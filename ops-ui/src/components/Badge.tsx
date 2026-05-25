import type { TicketPriority, TicketStatus, TicketCategory } from '../types';

interface BadgeProps {
  children: React.ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info';
  className?: string;
}

const variantClasses = {
  default: 'bg-gray-100 text-gray-700',
  success: 'bg-green-100 text-green-700',
  warning: 'bg-yellow-100 text-yellow-800',
  danger: 'bg-red-100 text-red-700',
  info: 'bg-blue-100 text-blue-700',
};

export function Badge({ children, variant = 'default', className = '' }: BadgeProps) {
  return (
    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${variantClasses[variant]} ${className}`}>
      {children}
    </span>
  );
}

export function PriorityBadge({ priority }: { priority: TicketPriority }) {
  const variant = {
    Critical: 'danger',
    High: 'warning',
    Medium: 'info',
    Low: 'default',
  }[priority] as BadgeProps['variant'];

  return <Badge variant={variant}>{priority}</Badge>;
}

export function StatusBadge({ status }: { status: TicketStatus }) {
  const variant = {
    New: 'info',
    Open: 'warning',
    InProgress: 'info',
    Pending: 'default',
    Resolved: 'success',
    Closed: 'default',
  }[status] as BadgeProps['variant'];

  const label = status === 'InProgress' ? 'In Progress' : status;
  return <Badge variant={variant}>{label}</Badge>;
}

export function CategoryBadge({ category }: { category: TicketCategory }) {
  const label = category === 'AccessPermissions' ? 'Access' :
    category === 'AccountManagement' ? 'Account' : category;
  return <Badge variant="default">{label}</Badge>;
}
