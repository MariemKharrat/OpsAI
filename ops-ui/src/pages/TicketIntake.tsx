import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Send, Bot, Loader2 } from 'lucide-react';
import { createTicket, triageTicket } from '../services/api';
import { Card } from '../components/Card';
import type { TicketCategory, TicketPriority } from '../types';

const categories: { value: TicketCategory; label: string }[] = [
  { value: 'Hardware', label: 'Hardware' },
  { value: 'Software', label: 'Software' },
  { value: 'Network', label: 'Network' },
  { value: 'Security', label: 'Security' },
  { value: 'AccessPermissions', label: 'Access & Permissions' },
  { value: 'Email', label: 'Email' },
  { value: 'Printing', label: 'Printing' },
  { value: 'VPN', label: 'VPN' },
  { value: 'AccountManagement', label: 'Account Management' },
  { value: 'Other', label: 'Other' },
];

const priorities: { value: TicketPriority; label: string }[] = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
];

export default function TicketIntake() {
  const navigate = useNavigate();
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    subject: '',
    description: '',
    requesterName: '',
    requesterEmail: '',
    requesterDepartment: '',
    category: '' as string,
    priority: '' as string,
    triggerAiTriage: true,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);

    try {
      const res = await createTicket({
        ...form,
        category: form.category || undefined,
        priority: form.priority || undefined,
        triggerAiTriage: form.triggerAiTriage,
      });

      const ticketId = res.data.id;

      if (form.triggerAiTriage) {
        await triageTicket(ticketId);
        navigate(`/tickets/${ticketId}/triage`);
      } else {
        navigate(`/tickets/${ticketId}/workspace`);
      }
    } catch (err) {
      console.error('Failed to create ticket:', err);
      setSubmitting(false);
    }
  };

  const updateField = (field: string, value: string | boolean) =>
    setForm(prev => ({ ...prev, [field]: value }));

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Submit New Ticket</h1>
        <p className="text-gray-500 mt-1">Describe your IT issue and our AI copilot will assist with triage and resolution</p>
      </div>

      <Card>
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Requester Info */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Your Name *</label>
              <input
                type="text" required value={form.requesterName}
                onChange={e => updateField('requesterName', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="John Smith"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Email *</label>
              <input
                type="email" required value={form.requesterEmail}
                onChange={e => updateField('requesterEmail', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="john@company.com"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Department *</label>
              <input
                type="text" required value={form.requesterDepartment}
                onChange={e => updateField('requesterDepartment', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Engineering"
              />
            </div>
          </div>

          {/* Subject */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Subject *</label>
            <input
              type="text" required value={form.subject}
              onChange={e => updateField('subject', e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Brief summary of the issue..."
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Description *</label>
            <textarea
              required rows={5} value={form.description}
              onChange={e => updateField('description', e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Provide details: what happened, when it started, what you've tried, impact on your work..."
            />
          </div>

          {/* Category & Priority */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Category (optional)</label>
              <select
                value={form.category}
                onChange={e => updateField('category', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Let AI classify</option>
                {categories.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Priority (optional)</label>
              <select
                value={form.priority}
                onChange={e => updateField('priority', e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Let AI assess</option>
                {priorities.map(p => <option key={p.value} value={p.value}>{p.label}</option>)}
              </select>
            </div>
          </div>

          {/* AI Triage Toggle */}
          <div className="flex items-center gap-3 p-4 bg-blue-50 rounded-lg border border-blue-100">
            <Bot className="w-5 h-5 text-blue-600 flex-shrink-0" />
            <div className="flex-1">
              <p className="text-sm font-medium text-blue-900">AI-Powered Triage</p>
              <p className="text-xs text-blue-700">Automatically classify priority, category, and find relevant knowledge articles</p>
            </div>
            <label className="relative inline-flex items-center cursor-pointer">
              <input type="checkbox" checked={form.triggerAiTriage}
                onChange={e => updateField('triggerAiTriage', e.target.checked)}
                className="sr-only peer" />
              <div className="w-11 h-6 bg-gray-200 peer-focus:ring-2 peer-focus:ring-blue-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-blue-600" />
            </label>
          </div>

          {/* Submit */}
          <div className="flex justify-end">
            <button type="submit" disabled={submitting}
              className="inline-flex items-center gap-2 px-6 py-2.5 bg-blue-600 text-white rounded-lg font-medium text-sm hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
              {submitting ? (
                <><Loader2 className="w-4 h-4 animate-spin" /> Processing...</>
              ) : (
                <><Send className="w-4 h-4" /> Submit Ticket</>
              )}
            </button>
          </div>
        </form>
      </Card>
    </div>
  );
}
