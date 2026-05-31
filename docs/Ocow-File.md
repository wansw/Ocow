# 开发需求：文件上传下载、订单报表导出、产品批量导入功能

## 一、项目背景

当前项目是 C# .NET Core 微服务架构，数据库使用 PostgreSQL。需要新增以下能力：

1. 文件上传和下载能力，支持 txt、Excel、图片。
2. 订单报表导出能力，订单数据来自 PostgreSQL，可能涉及多表关联，导出数据量可能很大。
3. 产品批量导入能力，产品文件包含产品编号、产品名称，文件本身没有业务价值，主要目的是读取文件内容并保存到数据库。
4. 大文件导入时，需要支持异步任务、进度查询、错误明细下载。

系统后续可能接入腾讯云 COS，用于保存上传文件、导出报表文件、导入错误文件等。

---

# 二、技术栈要求

后端：

* C# .NET Core / ASP.NET Core
* PostgreSQL
* Npgsql
* BackgroundService 或独立 Worker
* 支持微服务拆分
* 支持腾讯云 COS SDK，后续用于文件存储

文件处理：

* Excel 读取可使用 ExcelDataReader / MiniExcel / ClosedXML 等
* 大文件建议优先支持 CSV
* PostgreSQL 批量入库优先使用 Npgsql COPY
* 大文件导出优先使用 CSV

---

# 三、文件上传和下载功能

## 3.1 支持的文件类型

上传支持：

* txt
* xls
* xlsx
* jpg
* jpeg
* png
* webp

下载支持：

* txt
* xls
* xlsx

图片上传后需要返回访问地址或临时访问地址。

---

## 3.2 文件存储方案

第一版可以支持本地存储，最终建议支持腾讯云 COS。

推荐设计：

```text
前端
  ↓
后端接口
  ↓
文件校验
  ↓
上传到本地或 COS
  ↓
保存文件元数据到数据库
```

如果使用 COS：

* Bucket 设置为私有读写
* 数据库保存 object_key，不要只保存完整 URL
* 下载时由后端生成 5 分钟有效的预签名 URL
* 图片如果是公开图片，后续可以接 CDN
* 临时文件可通过 COS 生命周期自动删除

当前上传实现通过 `FileStorage:StorageType` 选择存储方式：

```json
{
  "FileStorage": {
    "StorageType": "Local",
    "LocalRootPath": ".appdata/files",
    "CosBucketName": "",
    "CosRegion": "ap-guangzhou",
    "CosSecretId": "",
    "CosSecretKey": "",
    "CosKeyPrefix": "uploads"
  }
}
```

配置说明：

* `StorageType=Local`：上传文件保存到本地 `LocalRootPath`。
* `StorageType=TencentCos` 或 `StorageType=Cos`：上传文件保存到腾讯 COS。
* 使用 COS 时必须配置 `CosBucketName`、`CosRegion`、`CosSecretId`、`CosSecretKey`。
* Docker 部署可通过 `OCOW_FILE_STORAGE_TYPE` 和 `OCOW_FILE_COS_*` 环境变量切换。

---

## 3.3 文件元数据表

```sql
CREATE TABLE file_resource (
    id BIGSERIAL PRIMARY KEY,
    original_name VARCHAR(255) NOT NULL,
    object_key VARCHAR(500) NOT NULL,
    bucket_name VARCHAR(100),
    region VARCHAR(50),
    file_type VARCHAR(50) NOT NULL,
    mime_type VARCHAR(100),
    extension VARCHAR(20),
    file_size BIGINT,
    storage_type VARCHAR(20) DEFAULT 'local',
    biz_type VARCHAR(50),
    biz_id BIGINT,
    uploader_id BIGINT,
    status VARCHAR(30) DEFAULT 'normal',
    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now()
);
```

---

## 3.4 上传接口

```http
POST /api/files/upload
Content-Type: multipart/form-data
```

参数：

```text
file: 上传文件
bizType: 业务类型，可选
bizId: 业务 ID，可选
```

返回：

```json
{
  "code": 200,
  "message": "上传成功",
  "data": {
    "fileId": 123,
    "fileName": "test.xlsx",
    "fileType": "excel",
    "fileSize": 102400
  }
}
```

---

## 3.5 下载接口

获取下载链接：

```http
GET /api/files/{fileId}/download-url
```

返回：

```json
{
  "code": 200,
  "data": {
    "url": "临时下载地址",
    "expireSeconds": 300
  }
}
```

如果是敏感文件，可以支持后端代理下载：

```http
GET /api/files/{fileId}/download
```

---

## 3.6 文件安全校验

后端必须校验：

* 文件大小
* 文件后缀
* MIME Type
* 图片真实格式
* 禁止 exe、sh、bat、js、html、php、jsp、jar、class 等危险文件
* 保存文件时使用 UUID 文件名，不能直接使用原始文件名作为存储路径

建议限制：

```text
txt: 2MB
Excel: 20MB
图片: 10MB
```

---

# 四、订单报表导出功能

## 4.1 场景说明

订单报表数据来自 PostgreSQL，可能涉及多表关联，例如：

* orders
* order_items
* users
* products
* payments
* shipments
* refunds

导出时间范围可能很长，数据量可能很大，所以不能做同步接口直接下载。

禁止实现方式：

```text
接口中直接查询全部数据
一次性 ToListAsync()
一次性生成 Excel
直接返回文件流
```

推荐实现方式：

```text
异步导出任务
+ 分批读取 PostgreSQL
+ 流式生成 CSV / Excel
+ 上传 COS 或文件服务
+ 前端轮询任务状态
+ 完成后获取下载链接
```

---

## 4.2 导出任务表

```sql
CREATE TABLE report_export_task (
    id BIGSERIAL PRIMARY KEY,
    task_no VARCHAR(64) NOT NULL UNIQUE,
    report_type VARCHAR(50) NOT NULL,
    status VARCHAR(30) NOT NULL,
    file_name VARCHAR(255),
    object_key VARCHAR(500),
    file_size BIGINT,
    query_params JSONB,
    total_count BIGINT DEFAULT 0,
    exported_count BIGINT DEFAULT 0,
    error_message TEXT,
    created_by BIGINT,
    created_at TIMESTAMP DEFAULT now(),
    started_at TIMESTAMP,
    finished_at TIMESTAMP
);
```

状态：

```text
PENDING
PROCESSING
SUCCESS
FAILED
EXPIRED
```

---

## 4.3 创建导出任务接口

```http
POST /api/reports/orders/export
```

请求：

```json
{
  "startDate": "2026-05-01",
  "endDate": "2026-05-30",
  "format": "csv",
  "orderStatus": "paid"
}
```

返回：

```json
{
  "code": 200,
  "data": {
    "taskId": 10001,
    "taskNo": "EXP202605300001",
    "status": "PENDING"
  }
}
```

---

## 4.4 查询导出任务状态

```http
GET /api/reports/export-tasks/{taskId}
```

返回：

```json
{
  "code": 200,
  "data": {
    "taskId": 10001,
    "status": "PROCESSING",
    "totalCount": 1200000,
    "exportedCount": 300000,
    "progress": 25
  }
}
```

---

## 4.5 获取导出文件下载链接

```http
GET /api/reports/export-tasks/{taskId}/download-url
```

返回：

```json
{
  "code": 200,
  "data": {
    "url": "COS 预签名下载地址",
    "expireSeconds": 300
  }
}
```

---

## 4.6 订单报表数据读取方式

优先使用 DataReader 或 PostgreSQL COPY，禁止一次性加载所有数据。

示例方式：

```text
Npgsql DataReader
CommandBehavior.SequentialAccess
StreamWriter 流式写 CSV
每处理 5000 行更新一次进度
```

如果只是导出 CSV，可以使用 PostgreSQL COPY TO STDOUT 提升性能。

---

## 4.7 多表关联处理方式

短期方案：

```text
异步导出任务
+ 按订单主表游标分页
+ 每批批量查询用户、商品、支付、物流等关联数据
+ 内存组装当前批次
+ 流式写 CSV
```

不要直接用一条巨大 JOIN 查询所有数据。

分页方式不要用 OFFSET，使用游标分页：

```sql
SELECT *
FROM orders
WHERE created_at >= @startDate
  AND created_at < @endDate
  AND id > @lastId
ORDER BY id
LIMIT @pageSize;
```

每批处理后：

```text
提取 orderIds、userIds、productIds
批量查询关联表
组装当前批次报表行
写入 CSV
释放内存
```

长期方案：

```text
建立订单报表宽表 order_report_detail
导出时只查询宽表
```

---

## 4.8 订单报表宽表建议

```sql
CREATE TABLE order_report_detail (
    id BIGSERIAL PRIMARY KEY,

    order_id BIGINT NOT NULL,
    order_no VARCHAR(64) NOT NULL,
    order_time TIMESTAMP NOT NULL,

    user_id BIGINT,
    user_name VARCHAR(100),
    user_mobile VARCHAR(50),

    shop_id BIGINT,
    shop_name VARCHAR(100),

    product_id BIGINT,
    product_name VARCHAR(255),
    sku_id BIGINT,
    sku_name VARCHAR(255),
    category_name VARCHAR(255),

    quantity INT,
    sale_price NUMERIC(18, 2),
    item_amount NUMERIC(18, 2),

    order_amount NUMERIC(18, 2),
    discount_amount NUMERIC(18, 2),
    pay_amount NUMERIC(18, 2),

    pay_no VARCHAR(64),
    pay_channel VARCHAR(50),
    pay_time TIMESTAMP,

    express_company VARCHAR(100),
    express_no VARCHAR(100),
    shipped_at TIMESTAMP,

    order_status VARCHAR(50),
    refund_status VARCHAR(50),

    created_at TIMESTAMP DEFAULT now(),
    updated_at TIMESTAMP DEFAULT now()
);
```

索引：

```sql
CREATE INDEX idx_order_report_detail_order_time_id
ON order_report_detail (order_time, id);

CREATE INDEX idx_order_report_detail_shop_time
ON order_report_detail (shop_id, order_time);

CREATE INDEX idx_order_report_detail_user_time
ON order_report_detail (user_id, order_time);

CREATE INDEX idx_order_report_detail_product_time
ON order_report_detail (product_id, order_time);
```

---

## 4.9 报表格式规则

优先 CSV。

```text
10 万行以内：可以支持 xlsx
10 万 - 100 万行：推荐 CSV
超过 100 万行：CSV 或 CSV.GZ
Excel 超过单 Sheet 行数限制时需要拆 Sheet 或拆文件
```

---

# 五、产品导入功能

## 5.1 场景说明

产品导入文件包含：

```text
产品编号
产品名称
```

Excel 文件本身没有业务保存价值，普通小文件可以不保存原文件，后端接收后直接读取、校验、入库。

但是如果文件很大，则需要临时保存到本地或 COS，供后台 Worker 异步处理。处理完成后可以通过生命周期或定时任务删除原文件。

---

## 5.2 产品表

单租户版本：

```sql
CREATE TABLE product (
    id BIGSERIAL PRIMARY KEY,
    product_code VARCHAR(64) NOT NULL UNIQUE,
    product_name VARCHAR(255) NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'active',
    created_at TIMESTAMP NOT NULL DEFAULT now(),
    updated_at TIMESTAMP NOT NULL DEFAULT now()
);
```

多租户版本：

```sql
CREATE TABLE product (
    id BIGSERIAL PRIMARY KEY,
    tenant_id BIGINT NOT NULL,
    product_code VARCHAR(64) NOT NULL,
    product_name VARCHAR(255) NOT NULL,
    status VARCHAR(30) NOT NULL DEFAULT 'active',
    created_at TIMESTAMP NOT NULL DEFAULT now(),
    updated_at TIMESTAMP NOT NULL DEFAULT now(),
    UNIQUE (tenant_id, product_code)
);
```

如果系统有租户，需要使用多租户版本。

---

## 5.3 小文件同步导入接口

```http
POST /api/products/import
Content-Type: multipart/form-data
```

参数：

```text
file: Excel 文件
mode: insert / upsert
```

返回：

```json
{
  "code": 200,
  "message": "导入完成",
  "data": {
    "totalRows": 100,
    "successRows": 95,
    "failedRows": 5,
    "errors": [
      {
        "rowIndex": 3,
        "message": "产品编号不能为空"
      },
      {
        "rowIndex": 8,
        "message": "产品编号在文件中重复"
      }
    ]
  }
}
```

---

## 5.4 Excel 模板

表头固定为：

```text
产品编号, 产品名称
```

第 1 行为表头，第 2 行开始为数据。

字段规则：

```text
产品编号：必填，最大 64 字符，唯一
产品名称：必填，最大 255 字符
```

---

## 5.5 导入模式

支持两种模式：

### insert

只新增。

```text
如果产品编号已存在，返回错误，不覆盖数据库数据
```

### upsert

新增或更新。

```text
如果产品编号已存在，更新产品名称
如果产品编号不存在，新增产品
```

PostgreSQL upsert 示例：

```sql
INSERT INTO product (
    product_code,
    product_name,
    status,
    created_at,
    updated_at
)
VALUES (
    @productCode,
    @productName,
    'active',
    now(),
    now()
)
ON CONFLICT (product_code)
DO UPDATE SET
    product_name = EXCLUDED.product_name,
    updated_at = now();
```

多租户版本：

```sql
INSERT INTO product (
    tenant_id,
    product_code,
    product_name,
    status,
    created_at,
    updated_at
)
VALUES (
    @tenantId,
    @productCode,
    @productName,
    'active',
    now(),
    now()
)
ON CONFLICT (tenant_id, product_code)
DO UPDATE SET
    product_name = EXCLUDED.product_name,
    updated_at = now();
```

---

# 六、大文件产品导入功能

## 6.1 适用场景

如果产品文件很大，例如：

```text
几万行
几十万行
上百万行
```

不能使用同步接口直接导入。

需要做成：

```text
异步导入任务
+ 临时保存文件
+ 后台 Worker 流式解析
+ 分批校验
+ 批量写入 PostgreSQL
+ 前端查询进度
+ 错误文件下载
```

---

## 6.2 文件格式建议

大文件优先支持 CSV。

建议规则：

```text
5000 行以内：Excel 同步导入
5000 - 10 万行：Excel 异步导入
10 万行以上：CSV 异步导入
50 万行以上：强制 CSV
```

Excel 大文件不要使用 DataTable 或 AsDataSet，不要一次性加载到内存。

---

## 6.3 导入任务表

```sql
CREATE TABLE product_import_task (
    id BIGSERIAL PRIMARY KEY,
    task_no VARCHAR(64) NOT NULL UNIQUE,
    status VARCHAR(30) NOT NULL,

    file_name VARCHAR(255),
    file_path VARCHAR(500),
    file_type VARCHAR(20),
    file_size BIGINT,

    import_mode VARCHAR(20) NOT NULL,

    total_rows BIGINT DEFAULT 0,
    processed_rows BIGINT DEFAULT 0,
    success_rows BIGINT DEFAULT 0,
    failed_rows BIGINT DEFAULT 0,

    error_file_path VARCHAR(500),
    error_message TEXT,

    created_by BIGINT,
    created_at TIMESTAMP NOT NULL DEFAULT now(),
    started_at TIMESTAMP,
    finished_at TIMESTAMP
);
```

状态：

```text
PENDING
PROCESSING
SUCCESS
PARTIAL_SUCCESS
FAILED
CANCELLED
```

---

## 6.4 导入明细表

```sql
CREATE TABLE product_import_detail (
    id BIGSERIAL PRIMARY KEY,
    task_id BIGINT NOT NULL,

    row_index BIGINT NOT NULL,
    product_code VARCHAR(64),
    product_name VARCHAR(255),

    is_valid BOOLEAN DEFAULT true,
    error_message TEXT,

    created_at TIMESTAMP NOT NULL DEFAULT now()
);
```

---

## 6.5 创建大文件导入任务接口

```http
POST /api/products/import-tasks
Content-Type: multipart/form-data
```

参数：

```text
file: Excel 或 CSV
mode: insert / upsert
```

返回：

```json
{
  "code": 200,
  "data": {
    "taskId": 10001,
    "status": "PENDING"
  }
}
```

接口只负责：

```text
接收文件
保存临时文件
创建任务
返回 taskId
```

不要在这个接口里直接导入数据库。

---

## 6.6 查询导入任务状态接口

```http
GET /api/products/import-tasks/{taskId}
```

返回：

```json
{
  "code": 200,
  "data": {
    "taskId": 10001,
    "status": "PROCESSING",
    "totalRows": 500000,
    "processedRows": 120000,
    "successRows": 119500,
    "failedRows": 500,
    "progress": 24
  }
}
```

---

## 6.7 下载错误明细接口

```http
GET /api/products/import-tasks/{taskId}/error-file
```

错误文件内容格式：

```csv
行号,产品编号,产品名称,错误原因
3,,苹果,产品编号不能为空
8,P0001,香蕉,产品编号在文件中重复
15,P0009,,产品名称不能为空
```

---

## 6.8 后台 Worker 处理流程

```text
1. 查询 PENDING 任务
2. 更新任务状态为 PROCESSING
3. 打开临时文件流
4. 逐行读取 Excel 或 CSV
5. 每 5000 行作为一批
6. 校验空值、长度、基础格式
7. 批量写入 product_import_detail
8. 更新 processed_rows
9. 文件读取完成后，做全局重复校验
10. 校验数据库是否已存在
11. insert 模式下，已存在产品编号标记错误
12. upsert 模式下，允许更新
13. 如果是严格模式，有错误则不写 product 表，生成错误文件
14. 如果无错误，将合法数据批量 upsert 到 product 表
15. 更新 success_rows、failed_rows、status
16. 原始文件后续自动清理
```

---

## 6.9 文件内重复校验 SQL

```sql
UPDATE product_import_detail d
SET is_valid = false,
    error_message = COALESCE(error_message || ';', '') || '产品编号在文件中重复'
FROM (
    SELECT task_id, product_code
    FROM product_import_detail
    WHERE task_id = @taskId
      AND product_code IS NOT NULL
      AND product_code <> ''
    GROUP BY task_id, product_code
    HAVING count(*) > 1
) dup
WHERE d.task_id = dup.task_id
  AND d.product_code = dup.product_code;
```

---

## 6.10 数据库已存在校验 SQL

insert 模式：

```sql
UPDATE product_import_detail d
SET is_valid = false,
    error_message = COALESCE(error_message || ';', '') || '产品编号已存在'
FROM product p
WHERE d.task_id = @taskId
  AND d.product_code = p.product_code;
```

多租户版本：

```sql
UPDATE product_import_detail d
SET is_valid = false,
    error_message = COALESCE(error_message || ';', '') || '产品编号已存在'
FROM product p
WHERE d.task_id = @taskId
  AND d.product_code = p.product_code
  AND p.tenant_id = @tenantId;
```

---

## 6.11 从导入明细表批量 upsert 到产品表

单租户：

```sql
INSERT INTO product (
    product_code,
    product_name,
    status,
    created_at,
    updated_at
)
SELECT
    product_code,
    product_name,
    'active',
    now(),
    now()
FROM product_import_detail
WHERE task_id = @taskId
  AND is_valid = true
ON CONFLICT (product_code)
DO UPDATE SET
    product_name = EXCLUDED.product_name,
    updated_at = now();
```

多租户：

```sql
INSERT INTO product (
    tenant_id,
    product_code,
    product_name,
    status,
    created_at,
    updated_at
)
SELECT
    @tenantId,
    product_code,
    product_name,
    'active',
    now(),
    now()
FROM product_import_detail
WHERE task_id = @taskId
  AND is_valid = true
ON CONFLICT (tenant_id, product_code)
DO UPDATE SET
    product_name = EXCLUDED.product_name,
    updated_at = now();
```

---

# 七、后台任务实现要求

可以先使用 ASP.NET Core BackgroundService 实现。

后续如果导出、导入任务变多，建议拆成独立 Worker 服务：

```text
api-service
  创建任务
  查询任务状态
  获取下载链接

worker-service
  扫描 PENDING 任务
  执行导入 / 导出
  更新任务状态
```

Worker 获取任务时需要避免多实例重复消费，可以使用：

```sql
SELECT *
FROM product_import_task
WHERE status = 'PENDING'
ORDER BY created_at
LIMIT 1
FOR UPDATE SKIP LOCKED;
```

订单导出任务也可以用类似机制。

---

# 八、并发和限制要求

需要加限制，避免大文件拖垮系统：

```text
每个用户最多 1 个进行中的导入任务
每个用户最多 1 个进行中的导出任务
全局 Worker 并发 2 - 5 个
Excel 最大 10 万行
CSV 最大 100 万行
单批处理 2000 - 10000 行，默认 5000
导出时间范围默认最多 31 天
导出文件保存 7 - 30 天
导入原始文件保存 1 - 7 天
错误文件保存 7 - 30 天
```

---

# 九、前端交互要求

## 9.1 导出报表

```text
1. 用户选择订单时间范围和筛选条件
2. 点击导出
3. 后端返回 taskId
4. 页面显示导出中
5. 每 3 秒轮询任务状态
6. 成功后显示下载按钮
7. 点击下载按钮，获取临时下载链接
```

---

## 9.2 产品导入

小文件：

```text
1. 上传 Excel
2. 后端同步校验并导入
3. 返回成功数量和错误明细
```

大文件：

```text
1. 上传 Excel 或 CSV
2. 后端返回 importTaskId
3. 页面显示导入中
4. 每 3 秒轮询进度
5. 成功后显示导入成功数量
6. 失败或部分失败时显示错误文件下载按钮
```

---

# 十、实现优先级

## 第一阶段

必须完成：

```text
1. 文件上传接口
2. 文件下载链接接口
3. file_resource 表
4. 产品小文件 Excel 导入
5. 产品导入校验：表头、空值、长度、重复
6. PostgreSQL upsert
7. 订单报表异步导出任务
8. CSV 导出
9. 导出文件上传到本地或 COS
10. 导出任务状态查询
```

---

## 第二阶段

增强能力：

```text
1. 大文件产品异步导入
2. product_import_task 表
3. product_import_detail 表
4. CSV 大文件导入
5. Npgsql COPY 批量写入
6. 错误明细文件下载
7. BackgroundService / Worker
8. COS 生命周期清理
```

---

## 第三阶段

性能优化：

```text
1. 订单报表宽表 order_report_detail
2. 订单、支付、商品、用户等数据同步到报表宽表
3. 导出只查宽表
4. CSV.GZ 压缩导出
5. CDN 加速图片访问
6. 任务取消和失败重试
7. 更细的导入导出并发控制
```

---

# 十一、验收标准

## 文件上传下载

```text
支持 txt、xls、xlsx、jpg、jpeg、png、webp 上传
非法文件类型会被拒绝
超出大小限制会被拒绝
上传成功后保存文件元数据
下载接口能返回有效下载链接
私有文件不能被未授权访问
```

---

## 订单报表导出

```text
创建导出任务后接口立即返回 taskId
大数据量导出不会导致接口超时
导出过程可以查询进度
导出完成后可以下载文件
导出失败能记录失败原因
导出过程不一次性加载所有订单数据到内存
```

---

## 产品导入

```text
支持产品编号、产品名称模板导入
表头错误会被拦截
产品编号为空会返回错误
产品名称为空会返回错误
产品编号重复会返回错误
insert 模式下，已存在产品编号返回错误
upsert 模式下，已存在产品编号更新产品名称
大文件导入不阻塞接口
大文件导入可以查询进度
导入错误可以下载错误明细文件
```

---

# 十二、关键实现原则

```text
1. 普通上传下载是文件管理场景，需要保存文件和元数据。
2. 产品导入是数据导入场景，小文件不需要保存原始 Excel。
3. 大文件导入需要临时保存文件，因为后台 Worker 要异步处理。
4. 订单导出不能同步接口直接返回大文件，必须使用异步任务。
5. 大数据导入导出优先 CSV，不优先 Excel。
6. PostgreSQL 大批量写入使用 COPY 或 INSERT SELECT。
7. 任何大数据处理都不能一次性 ToListAsync 加载全部数据。
8. COS 中保存 object_key，下载时生成短期预签名 URL。
9. 数据库要保存任务状态、处理进度、失败原因。
10. 前端通过轮询任务状态获取导入导出结果。
```
