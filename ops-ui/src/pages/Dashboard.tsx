import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { BarChart3, AlertTriangle, CheckCircle2, Clock, Ticket, Filter, X } from 'lucide-react';
import { getStats, getTickets } from '../services/api';
import { StatCard, Card } from '../components/Card';
import { PriorityBadge, StatusBadge, CategoryBadge } from '../components/Badge';
import type { DashboardStats, Ticket as TicketType, TicketStatus, TicketPriority, TicketCategory } from '../types';

const STATUSES: TicketStatus[] = ['New', 'Open', 'InProgress', 'Pending', 'Resolved', 'Closed'];
const PRIORITIES: TicketPriority[] = ['Critical', 'High', 'Medium', 'Low'];
const CATEGORIES: TicketCategory[] = [
  'Hardware', 'Software', 'Network', 'Security', 'AccessPermissions',
  'Email', 'Printing', 'VPN', 'AccountManagement', 'Other',
];

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [filteredTickets, setFilteredTickets] = useState<TicketType[]>([]);
  const [filterStatus, setFilterStatus] = useState('');
  const [filterPriority, setFilterPriority] = useState('');
  const [filterCategory, setFilterCategory] = useState('');
  const [filtering, setFiltering] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    getStats().then(r => { setStats(r.data); setFilteredTickets(r.data.recentTickets); setLoading(false); })
      .catch(() => setLoading(false));
  }, []);

  const hasFilters = filterStatus || filterPriority || filterCategory;

  useEffect(() => {
    if (!stats) return;
    if (!hasFilters) {
      setFilteredTickets(stats.recentTickets);
      return;
    }
    setFiltering(true);
    getTickets({
      status: filterStatus || undefined,
      priority: filterPriority || undefined,
      category: filterCategory || undefined,
      page: 1,
      pageSize: 20,
    }).then(res => {
      setFilteredTickets(res.data.items);
    }).catch(() => {
      setFilteredTickets([]);
    }).finally(() => setFiltering(false));
  }, [filterStatus, filterPriority, filterCategory]);

  const clearFilters = () => {
    setFilterStatus('');
    setFilterPriority('');
    setFilterCategory('');
  };

  if (loading) return <LoadingSkeleton />;
  if (!stats) return <p className="text-red-500">Failed to load dashboard data.</p>;

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-500 mt-1">IT Support Operations Overview</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total Tickets"
          value={stats.totalTickets}
          icon={<Ticket className="w-6 h-6 text-blue-600" />}
        />
        <StatCard
          label="Open Tickets"
          value={stats.openTickets}
          icon={<Clock className="w-6 h-6 text-blue-600" />}
        />
        <StatCard
          label="Critical Issues"
          value={stats.criticalTickets}
          icon={<AlertTriangle className="w-6 h-6 text-red-500" />}
        />
        <StatCard
          label="Resolved Today"
          value={stats.resolvedToday}
          icon={<CheckCircle2 className="w-6 h-6 text-green-500" />}
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card title="By Status">
          <div className="space-y-3">
            {Object.entries(stats.byStatus).map(([status, count]) => (
              <div key={status} className="flex items-center justify-between">
                <span className="text-sm text-gray-600">{status}</span>
                <div className="flex items-center gap-2">
                  <div className="w-24 bg-gray-100 rounded-full h-2">
                    <div
                      className="bg-blue-500 h-2 rounded-full"
                      style={{ width: `${(count / stats.totalTickets) * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium w-6 text-right">{count}</span>
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card title="By Priority">
          <div className="space-y-3">
            {Object.entries(stats.byPriority).map(([priority, count]) => {
              const colors: Record<string, string> = { Critical: 'bg-red-500', High: 'bg-orange-500', Medium: 'bg-blue-500', Low: 'bg-gray-400' };
              return (
                <div key={priority} className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">{priority}</span>
                  <div className="flex items-center gap-2">
                    <div className="w-24 bg-gray-100 rounded-full h-2">
                      <div className={`${colors[priority] || 'bg-gray-400'} h-2 rounded-full`}
                        style={{ width: `${(count / stats.totalTickets) * 100}%` }} />
                    </div>
                    <span className="text-sm font-medium w-6 text-right">{count}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </Card>

        <Card title="By Category">
          <div className="space-y-3">
            {Object.entries(stats.byCategory).sort((a, b) => b[1] - a[1]).slice(0, 6).map(([cat, count]) => (
              <div key={cat} className="flex items-center justify-between">
                <span className="text-sm text-gray-600">{cat}</span>
                <span className="text-sm font-medium">{count}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      {/* Tickets with Filter Bar */}
      <Card title="Tickets" subtitle="Filter to find what needs attention"
        action={
          <button onClick={() => navigate('/tickets/new')}
            className="text-sm text-blue-600 hover:text-blue-700 font-medium">
            + New Ticket
          </button>
        }>
        {/* Filter Bar */}
        <div className="flex flex-wrap items-center gap-3 mb-4 pb-4 border-b border-gray-100">
          <div className="flex items-center gap-1.5 text-sm font-medium text-gray-700">
            <Filter className="w-4 h-4" />
            Filters
          </div>

          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Statuses</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>{s === 'InProgress' ? 'In Progress' : s}</option>
            ))}
          </select>

          <select
            value={filterPriority}
            onChange={(e) => setFilterPriority(e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Priorities</option>
            {PRIORITIES.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>

          <select
            value={filterCategory}
            onChange={(e) => setFilterCategory(e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="">All Categories</option>
            {CATEGORIES.map((c) => (
              <option key={c} value={c}>
                {c === 'AccessPermissions' ? 'Access & Permissions' :
                 c === 'AccountManagement' ? 'Account Management' : c}
              </option>
            ))}
          </select>

          {hasFilters && (
            <button
              onClick={clearFilters}
              className="flex items-center gap-1 px-3 py-1.5 text-sm text-red-600 hover:text-red-700 hover:bg-red-50 rounded-lg transition-colors"
            >
              <X className="w-3.5 h-3.5" />
              Clear
            </button>
          )}
        </div>

        {/* Ticket Table */}
        {filtering ? (
          <div className="space-y-3 animate-pulse">
            {[1, 2, 3].map((i) => <div key={i} className="h-10 bg-gray-100 rounded" />)}
          </div>
        ) : filteredTickets.length === 0 ? (
          <p className="text-center py-8 text-gray-500 text-sm">No tickets match the selected filters.</p>
        ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-gray-500 border-b">
                <th className="pb-3 font-medium">Ticket</th>
                <th className="pb-3 font-medium">Subject</th>
                <th className="pb-3 font-medium">Status</th>
                <th className="pb-3 font-medium">Priority</th>
                <th className="pb-3 font-medium">Category</th>
                <th className="pb-3 font-medium">Requester</th>
                <th className="pb-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {filteredTickets.map(ticket => (
                <tr key={ticket.id} className="hover:bg-gray-50">
                  <td className="py-3 font-mono text-xs text-gray-500">{ticket.ticketNumber}</td>
                  <td className="py-3 max-w-xs truncate">{ticket.subject}</td>
                  <td className="py-3"><StatusBadge status={ticket.status} /></td>
                  <td className="py-3"><PriorityBadge priority={ticket.priority} /></td>
                  <td className="py-3"><CategoryBadge category={ticket.category} /></td>
                  <td className="py-3 text-gray-600">{ticket.requesterName}</td>
                  <td className="py-3">
                    <button onClick={() => navigate(`/tickets/${ticket.id}/workspace`)}
                      className="text-blue-600 hover:text-blue-700 text-xs font-medium">
                      Open
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        )}
      </Card>
    </div>
  );
}

function LoadingSkeleton() {
  return (
    <div className="space-y-8 animate-pulse">
      <div className="h-8 w-48 bg-gray-200 rounded" />
      <div className="grid grid-cols-4 gap-4">
        {[1,2,3,4].map(i => <div key={i} className="h-24 bg-gray-200 rounded-xl" />)}
      </div>
      <div className="h-64 bg-gray-200 rounded-xl" />
    </div>
  );
}
