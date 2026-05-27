/**
 * 后台登录请求结构，对应 AdminLoginReqDto。
 */
export interface AdminLoginReqDto {
  userName: string;
  password: string;
}

/**
 * 后台认证 Token 响应结构，对应 AuthTokenResDto。
 */
export interface AuthTokenResDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  scope: string;
  permissions: string[];
}

/**
 * 本地登录会话结构，用于前端保存 Admin Token 和权限点。
 */
export interface AuthSession {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  permissions: string[];
}

/**
 * 后台管理员响应结构，对应 AdminUserResDto。
 */
export interface AdminUser {
  id: string;
  userName: string;
  displayName: string;
  status: 1 | 2;
}

/**
 * 后台管理员创建请求结构，对应 AdminUserReqDto。
 */
export interface AdminUserReqDto {
  userName: string;
  password: string;
  displayName: string;
  roleIds: string[];
}

/**
 * 角色保存请求结构，对应 RoleReqDto。
 */
export interface RoleReqDto {
  code: string;
  name: string;
}

/**
 * 角色响应结构，对应 RoleResDto。
 */
export interface Role {
  id: string;
  code: string;
  name: string;
}

/**
 * 权限点响应结构，对应 PermissionResDto。
 */
export interface Permission {
  id: string;
  code: string;
  name: string;
  module: string;
}

/**
 * 角色绑定权限点请求结构，对应 BindRolePermissionsReqDto。
 */
export interface BindRolePermissionsReqDto {
  permissionIds: string[];
}

/**
 * 菜单保存请求结构，对应 MenuReqDto。
 */
export interface MenuReqDto {
  parentId: string | null;
  code: string;
  name: string;
  type: 1 | 2;
  path?: string | null;
  component?: string | null;
  icon?: string | null;
  sort: number;
  permissionId?: string | null;
  isVisible: boolean;
  isEnabled: boolean;
}

/**
 * 菜单响应结构，对应 MenuResDto。
 */
export interface AdminMenu extends MenuReqDto {
  id: string;
  permissionCode?: string | null;
  children: AdminMenu[];
}

/**
 * 前端侧边栏导航节点。
 */
export interface NavigationItem {
  id: string;
  title: string;
  path: string;
  icon?: string | null;
  children: NavigationItem[];
}

/**
 * 下拉选择项结构。
 */
export interface SelectOption {
  label: string;
  value: string;
}

/**
 * 后台订单响应结构，对应 OrderResDto。
 */
export interface Order {
  id: string;
  customerId: string;
  orderNo: string;
  status: 1 | 2 | 3 | 4 | 5;
  totalAmount: number;
  createdAt: string;
}

/**
 * 订单发货请求结构，对应 ShipOrderReqDto。
 */
export interface ShipOrderReqDto {
  expressCompany: string;
  expressNo: string;
}

/**
 * 任务定义保存请求结构，对应 CreateJobDefinitionReqDto。
 */
export interface CreateJobDefinitionReqDto {
  jobName: string;
  jobType: string;
  cron: string;
  targetService: string;
  targetApi: string;
  httpMethod: string;
  requestBody?: string | null;
  enabled: boolean;
}

/**
 * 任务定义响应结构，对应 JobDefinitionResDto。
 */
export interface JobDefinition {
  id: string;
  jobCode: string;
  jobName: string;
  jobType: string;
  cron: string;
  enabled: boolean;
  targetService: string;
  targetApi: string;
  httpMethod: string;
}

/**
 * 手动触发任务响应结构，对应 TriggerJobResDto。
 */
export interface TriggerJobResDto {
  id: string;
  backgroundJobId: string;
}

/**
 * Hangfire Dashboard 会话响应结构。
 */
export interface DashboardSessionResDto {
  dashboardPath: string;
  expiresAt: string;
}

/**
 * 后台任务入队响应结构。
 */
export interface EnqueueJobResDto {
  jobId: string;
  name: string;
  dashboardPath: string;
}
