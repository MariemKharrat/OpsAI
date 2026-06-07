import { useEffect, useState, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Filter, X, Search } from 'lucide-react';
import { getTickets } from '../services/api';
import { Card } from '../components/Card';
import { PriorityBadge, StatusBadge, CategoryBadge } from '../components/Badge';
import type { Ticket, TicketStatus, TicketPriority, TicketCategory } from '../types';

const STATUSES: TicketStatus[] = ['New', 'Open', 'InProgress', 'Pending', 'Resolved', 'Closed'];
const PRIORITIES: TicketPriority[] = ['Critical', 'High', 'Medium', 'Low'];
const CATEGORIES: TicketCategory[] = [
  'Hardware', 'Software', 'Network', 'Security', 'AccessPermissions',
  'Email', 'Printing', 'VPN', 'AccountManagement', 'Other',
];

export default function TicketList() {
  const [searchParams, setSearchParams] = useSearchParams();
  const navigate = useNavigate();

  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const pageSize = 20;

  const status = searchParams.get('status') || '';
  const priority = searchParams.get('priority') || '';
  const category = searchParams.get('category') || '';

  const setFilter = (key: string, value: string) => {
    const params = new URLSearchParams(searchParams);
    if (value) {
      params.set(key, value);
    } else {
      params.delete(key);
    }
    setSearchParams(params);
    setPage(1);
  };

  const clearFilters = () => {
    setSearchParams({});
    setPage(1);
  };

  const hasFilters = status || priority || category;

  const fetchTickets = useCallback(async () => {
    setLoading(true);
    try {
      const res = await getTickets({
        status: status || undefined,
        priority: priority || undefined,
        category: category || undefined,
        page,
        pageSize,
      });
      setTickets(res.data.items);
      setTotalCount(res.data.totalCount);
    } catch {
      setTickets([]);
    } finally {
      setLoading(false);
    }
  }, [status, priority, category, page]);

  useEffect(() => { fetchTickets(); }, [fetchTickets]);

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Tickets</h1>
          <p className="text-gray-500 mt-1">
            {totalCount} ticket{totalCount !== 1 ? 's' : ''} found
          </p>
        </div>
        <button
          onClick={() => navigate('/tickets/new')}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors"
        >
          + New Ticket
        </button>
      </div>

      {/* Filter Bar */}
      <Card>
        <div className="flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2 text-sm font-medium text-gray-700">
            <Filter className="w-4 h-4" />
            Filters
          </div>

          {/* Status Filter */}
          <select
            value={status}
            onChange={(e) => setFilter('status', e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Statuses</option>
            {STATUSES.map((s) => (
              <option key={s} value={s}>
                {s === 'InProgress' ? 'In Progress' : s}
              </option>
            ))}
          </select>

          {/* Priority Filter */}
          <select
            value={priority}
            onChange={(e) => setFilter('priority', e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">All Priorities</option>
            {PRIORITIES.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </select>

          {/* Category Filter */}
          <select
            value={category}
            onChange={(e) => setFilter('category', e.target.value)}
            className="px-3 py-1.5 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
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
      </Card>

      {/* Ticket Table */}
      <Card>
        {loading ? (
          <div className="space-y-3 animate-pulse">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="h-10 bg-gray-100 rounded" />
            ))}
          </div>
        ) : tickets.length === 0 ? (
          <div className="text-center py-12 text-gray-500">
            <Search className="w-10 h-10 mx-auto mb-3 text-gray-300" />
            <p className="font-medium">No tickets found</p>
            <p className="text-sm mt-1">Try adjusting your filters</p>
          </div>
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
                  <th className="pb-3 font-medium">Created</th>
                  <th className="pb-3 font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {tickets.map((ticket) => (
                  <tr key={ticket.id} className="hover:bg-gray-50">
                    <td className="py-3 font-mono text-xs text-gray-500">
                      {ticket.ticketNumber}
                    </td>
                    <td className="py-3 max-w-xs truncate">{ticket.subject}</td>
                    <td className="py-3"><StatusBadge status={ticket.status} /></td>
                    <td className="py-3"><PriorityBadge priority={ticket.priority} /></td>
                    <td className="py-3"><CategoryBadge category={ticket.category} /></td>
                    <td className="py-3 text-gray-600">{ticket.requesterName}</td>
                    <td className="py-3 text-gray-500 text-xs">
                      {new Date(ticket.createdAt).toLocaleDateString()}
                    </td>
                    <td className="py-3">
                      <button
                        onClick={() => navigate(`/tickets/${ticket.id}/workspace`)}
                        className="text-blue-600 hover:text-blue-700 text-xs font-medium"
                      >
                        Open
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between mt-4 pt-4 border-t">
            <span className="text-sm text-gray-500">
              Page {page} of {totalPages}
            </span>
            <div className="flex gap-2">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1.5 text-sm border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Previous
              </button>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-3 py-1.5 text-sm border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </Card>
    </div>
  );
}
