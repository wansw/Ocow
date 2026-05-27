import type {
  CreateJobDefinitionReqDto,
  DashboardSessionResDto,
  EnqueueJobResDto,
  JobDefinition,
  TriggerJobResDto
} from '../types/admin';
import { adminRequest } from './http';

/**
 * 创建 Hangfire Dashboard 会话。
 */
export function createDashboardSession(): Promise<DashboardSessionResDto> {
  return adminRequest<DashboardSessionResDto>('/api/admin/jobs/dashboard-session', {
    method: 'POST'
  });
}

/**
 * 手动触发示例后台任务。
 */
export function enqueueSampleJob(): Promise<EnqueueJobResDto> {
  return adminRequest<EnqueueJobResDto>('/api/admin/jobs/sample', {
    method: 'POST'
  });
}

/**
 * 创建或更新动态任务定义。
 */
export function saveJobDefinition(reqDto: CreateJobDefinitionReqDto): Promise<JobDefinition> {
  return adminRequest<JobDefinition>('/api/admin/JobDefinition/definitions', {
    method: 'POST',
    body: JSON.stringify(reqDto)
  });
}

/**
 * 手动触发指定动态任务。
 */
export function triggerJobDefinition(id: string): Promise<TriggerJobResDto> {
  return adminRequest<TriggerJobResDto>(`/api/admin/JobDefinition/definitions/${id}/trigger`, {
    method: 'POST'
  });
}
