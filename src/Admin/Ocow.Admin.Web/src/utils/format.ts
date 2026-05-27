/**
 * 格式化日期时间文本。
 */
export function formatDateTime(value?: string): string {
  if (!value) {
    return '-';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString('zh-CN', { hour12: false });
}

/**
 * 格式化金额文本。
 */
export function formatMoney(value?: number): string {
  if (value === undefined || value === null) {
    return '-';
  }

  return new Intl.NumberFormat('zh-CN', {
    style: 'currency',
    currency: 'CNY'
  }).format(value);
}

/**
 * 返回管理员状态展示文本。
 */
export function formatAdminStatus(status: number): string {
  return status === 1 ? '启用' : '禁用';
}

/**
 * 返回订单状态展示文本。
 */
export function formatOrderStatus(status: number): string {
  const statusMap: Record<number, string> = {
    1: '待支付',
    2: '已支付',
    3: '已发货',
    4: '已完成',
    5: '已取消'
  };

  return statusMap[status] ?? `未知(${status})`;
}
