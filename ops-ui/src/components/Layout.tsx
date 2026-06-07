import type { ReactNode } from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, PlusCircle, Bot, Settings, List } from 'lucide-react';

interface Props { children: ReactNode }

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/tickets', label: 'All Tickets', icon: List },
  { to: '/tickets/new', label: 'New Ticket', icon: PlusCircle },
];

export default function Layout({ children }: Props) {
  return (
    <div className="flex h-screen overflow-hidden">
      {/* Sidebar */}
      <aside className="w-64 bg-gray-900 text-white flex flex-col">
        <div className="p-5 border-b border-gray-700">
          <div className="flex items-center gap-2">
            <Bot className="w-7 h-7 text-blue-400" />
            <h1 className="text-xl font-bold">OpsAI</h1>
          </div>
          <p className="text-xs text-gray-400 mt-1">IT Support Copilot</p>
        </div>
        <nav className="flex-1 p-4 space-y-1">
          {navItems.map(({ to, label, icon: Icon }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                `flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors ${
                  isActive
                    ? 'bg-blue-600 text-white'
                    : 'text-gray-300 hover:bg-gray-800 hover:text-white'
                }`
              }
            >
              <Icon className="w-4 h-4" />
              {label}
            </NavLink>
          ))}
        </nav>
        <div className="p-4 border-t border-gray-700">
          <div className="flex items-center gap-2 text-xs text-gray-400">
            <Settings className="w-3.5 h-3.5" />
            <span>Azure AI Foundry Connected</span>
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto bg-gray-50">
        <div className="p-8">
          {children}
        </div>
      </main>
    </div>
  );
}
