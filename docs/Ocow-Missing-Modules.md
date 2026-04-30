# Ocow 电商缺失模块补充清单

## 1. 结论

当前 Ocow 架构已经覆盖订单、商品、支付、会员、库存、微信、定时任务、网关、Redis、RabbitMQ、认证权限和日志追踪，核心交易闭环已经具备基础形态。

如果要让电商骨架更完整，还建议补充以下模块。建议按优先级分三批推进：

- MVP 必需模块：购物车、价格、营销、文件存储、通知。
- 第二阶段增强模块：售后、履约物流、搜索、业务配置、审计中心。
- 后期平台化模块：结算、租户商户、推荐、数据分析、风控。

## 2. MVP 必需模块

### 2.1 `Ocow.Cart.*` 购物车服务

职责：

- 购物车商品添加。
- 购物车商品删除。
- 购物车商品数量修改。
- 商品勾选和取消勾选。
- 购物车商品有效性校验。
- 下单前购物车快照生成。

建议项目：

```text
Ocow.Cart.Api
Ocow.Cart.Application
Ocow.Cart.Domain
Ocow.Cart.Infrastructure
Ocow.Cart.Migrations
```

典型接口：

```text
GET    /api/cart
POST   /api/cart/items
PUT    /api/cart/items/{id}
DELETE /api/cart/items/{id}
POST   /api/cart/items/{id}/select
POST   /api/cart/select-all
```

边界说明：

- 购物车不负责最终价格结算，价格计算调用 `Ocow.Pricing`。
- 购物车不直接扣库存，库存校验和锁定由 `Ocow.Inventory` 负责。
- 购物车不创建订单，订单创建由 `Ocow.Order` 负责。

### 2.2 `Ocow.Pricing.*` 价格服务

职责：

- 商品基础销售价。
- SKU 价格。
- 会员价。
- 活动价。
- 订单结算价格计算。
- 价格快照生成。

建议项目：

```text
Ocow.Pricing.Api
Ocow.Pricing.Application
Ocow.Pricing.Domain
Ocow.Pricing.Infrastructure
Ocow.Pricing.Migrations
```

典型接口：

```text
POST /internal/pricing/calculate-cart
POST /internal/pricing/calculate-order
GET  /api/admin/prices/products/{productId}
PUT  /api/admin/prices/skus/{skuId}
```

边界说明：

- 商品服务负责商品资料，不负责复杂价格计算。
- 订单服务保存下单时的价格快照，不实时依赖商品价格。
- 营销活动的优惠规则可由 `Ocow.Promotion` 提供，价格服务负责最终计算编排。

### 2.3 `Ocow.Promotion.*` 营销促销服务

职责：

- 优惠券。
- 满减。
- 折扣。
- 限时活动。
- 活动资格校验。
- 优惠试算。

建议项目：

```text
Ocow.Promotion.Api
Ocow.Promotion.Application
Ocow.Promotion.Domain
Ocow.Promotion.Infrastructure
Ocow.Promotion.Migrations
```

典型接口：

```text
GET  /api/coupons
POST /api/coupons/{id}/claim
GET  /api/admin/promotions
POST /api/admin/promotions
PUT  /api/admin/promotions/{id}
POST /internal/promotions/calculate
```

边界说明：

- 优惠规则归 `Ocow.Promotion`。
- 最终订单金额由 `Ocow.Pricing` 汇总计算。
- 订单服务只保存优惠结果和优惠快照。
- 防刷券、重复领券、活动库存需要 Redis 幂等和限流。

### 2.4 `Ocow.Storage` 文件/对象存储服务

职责：

- 商品图片上传。
- 富文本图片上传。
- 后台附件上传。
- 小程序素材上传。
- 文件访问 URL 生成。
- 文件类型、大小、扩展名校验。

建议项目：

```text
Ocow.Storage
```

如果后续文件管理复杂，可以拆成：

```text
Ocow.Storage.Api
Ocow.Storage.Application
Ocow.Storage.Infrastructure
```

典型接口：

```text
POST /api/admin/files/upload
GET  /api/admin/files
DELETE /api/admin/files/{id}
POST /internal/files/sign-upload
```

边界说明：

- 业务服务只保存文件 URL 或文件 ID。
- 文件二进制不建议直接存数据库。
- MVP 可先本地磁盘，正式环境建议对象存储。
- 需要校验文件类型，避免上传脚本、恶意文件。

### 2.5 `Ocow.Notification.*` 通知服务

职责：

- 统一通知编排。
- 微信订阅消息。
- 短信。
- 邮件。
- 站内信。
- 通知模板。
- 通知发送记录。
- 失败重试。

建议项目：

```text
Ocow.Notification.Api
Ocow.Notification.Application
Ocow.Notification.Domain
Ocow.Notification.Infrastructure
Ocow.Notification.Migrations
```

典型场景：

```text
订单支付成功通知
订单发货通知
退款成功通知
优惠券到账通知
后台任务失败告警
```

边界说明：

- `Ocow.WeChat` 负责调用微信官方接口。
- `Ocow.Notification` 负责编排何时通知、通知谁、用什么模板。
- 通知类任务优先通过 RabbitMQ 异步触发。

## 3. 第二阶段增强模块

### 3.1 `Ocow.AfterSales.*` 售后服务

职责：

- 退款申请。
- 退货申请。
- 换货申请。
- 售后审核。
- 售后状态流转。
- 售后原因和凭证。

边界说明：

- 售后流程归 `Ocow.AfterSales`。
- 资金退款由 `Ocow.Payment` 执行。
- 库存回补由 `Ocow.Inventory` 执行。
- 订单只保存售后相关状态摘要。

### 3.2 `Ocow.Fulfillment.*` 履约物流服务

职责：

- 发货单。
- 物流公司。
- 物流单号。
- 物流轨迹。
- 收货确认。
- 分批发货预留。

边界说明：

- 后台发货动作可由订单后台入口触发，但履约细节建议沉到 `Ocow.Fulfillment`。
- 订单服务保存订单履约状态。
- 物流轨迹同步可以交给 `Ocow.Scheduler` 定时任务。

### 3.3 `Ocow.Search.*` 搜索服务

职责：

- 商品关键词搜索。
- 类目筛选。
- 品牌筛选。
- 价格区间筛选。
- 排序。
- 搜索索引同步。

边界说明：

- MVP 可以先用数据库查询。
- 第二阶段可接 Elasticsearch 或 OpenSearch。
- 商品上下架、价格变化、库存变化需要同步搜索索引。

### 3.4 `Ocow.ConfigCenter` 业务配置中心

职责：

- 首页配置。
- 业务开关。
- 支付配置。
- 微信配置。
- 活动配置。
- 字典配置。

边界说明：

- 技术配置继续使用配置文件或配置中心。
- 运营可改的业务配置放 `Ocow.ConfigCenter`。
- 敏感配置需要加密存储或接入密钥管理。

### 3.5 `Ocow.Audit.*` 审计中心

职责：

- 后台操作日志。
- 登录日志。
- 敏感操作日志。
- 数据变更前后对比。
- 审计查询。

边界说明：

- MVP 可以先由各服务自己写审计日志。
- 第二阶段可统一沉淀为 `Ocow.Audit`。
- 高风险操作必须记录操作人、IP、UserAgent、TraceId、变更前后数据。

## 4. 后期平台化模块

### 4.1 `Ocow.Settlement.*` 结算服务

适用场景：

- 多商户。
- 分销。
- 佣金。
- 账期。
- 对账。
- 结算单。

单商户 B2C MVP 暂时不需要。

### 4.2 `Ocow.Tenant.*` 租户/商户服务

适用场景：

- 多租户。
- 多商户。
- 店铺管理。
- 商户权限隔离。
- 商户独立结算。

单商户商城暂时不建议引入，避免过度复杂。

### 4.3 `Ocow.Recommendation.*` 推荐服务

职责：

- 猜你喜欢。
- 热销推荐。
- 相似商品。
- 个性化推荐。

建议后期在商品、订单、用户行为数据稳定后再做。

### 4.4 `Ocow.Analytics.*` 数据分析服务

职责：

- 销售报表。
- 订单统计。
- 用户增长。
- 转化漏斗。
- 运营看板。

MVP 可以先做简单后台统计，后续再独立。

### 4.5 `Ocow.RiskControl.*` 风控服务

职责：

- 防刷单。
- 防薅券。
- 防恶意退款。
- 登录风险识别。
- 高频接口风控。
- 支付风控。

当出现营销活动、优惠券、支付退款等高风险业务后，需要逐步补齐。

## 5. 推荐补充优先级

第一优先级：

```text
Ocow.Cart
Ocow.Pricing
Ocow.Storage
```

原因：没有购物车、价格和文件存储，商城基础体验很难完整。

第二优先级：

```text
Ocow.Promotion
Ocow.Notification
```

原因：营销和通知会明显影响运营能力，但可以先做轻量 MVP。

第三优先级：

```text
Ocow.AfterSales
Ocow.Fulfillment
Ocow.Search
Ocow.ConfigCenter
Ocow.Audit
```

原因：这些是电商持续运营需要的增强能力。

后期再评估：

```text
Ocow.Settlement
Ocow.Tenant
Ocow.Recommendation
Ocow.Analytics
Ocow.RiskControl
```

原因：这些模块会显著增加复杂度，应在业务规模、运营模式和团队协作方式明确后再拆。

## 6. 最小落地建议

如果下一步只补最小必要范围，建议先加入：

```text
Ocow.Cart.*
Ocow.Pricing.*
Ocow.Storage
Ocow.Notification.*
```

`Ocow.Promotion.*` 可以先只预留项目和接口，不急着实现复杂规则。

