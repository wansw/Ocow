import type { PageReqDto, PageResDto } from '../types/api';
import type { Order, ShipOrderReqDto } from '../types/admin';
import { adminRequest } from './http';

/**
 * 查询后台订单分页列表。
 */
export function getOrders(reqDto: PageReqDto): Promise<PageResDto<Order>> {
  const query = new URLSearchParams({
    pageIndex: String(reqDto.pageIndex),
    pageSize: String(reqDto.pageSize)
  });
  return adminRequest<PageResDto<Order>>(`/api/admin/orders?${query.toString()}`);
}

/**
 * 查询后台订单详情。
 */
export function getOrder(id: string): Promise<Order> {
  return adminRequest<Order>(`/api/admin/orders/${id}`);
}

/**
 * 执行后台订单发货。
 */
export function shipOrder(id: string, reqDto: ShipOrderReqDto): Promise<Order> {
  return adminRequest<Order>(`/api/admin/orders/${id}/ship`, {
    method: 'PUT',
    body: JSON.stringify(reqDto)
  });
}
