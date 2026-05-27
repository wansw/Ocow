/**
 * 统一接口响应结构，对应后端 ApiResDto。
 */
export interface ApiResDto<T> {
  success: boolean;
  code: string;
  message: string;
  data?: T;
  traceId?: string;
}

/**
 * 分页请求参数，对应后端 PageReqDto。
 */
export interface PageReqDto {
  pageIndex: number;
  pageSize: number;
}

/**
 * 分页响应结构，对应后端 PageResDto。
 */
export interface PageResDto<T> {
  items: T[];
  total: number;
  pageIndex: number;
  pageSize: number;
}
