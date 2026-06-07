import { Routes, Route, Navigate } from 'react-router-dom';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import TicketList from './pages/TicketList';
import TicketIntake from './pages/TicketIntake';
import TriageResultPage from './pages/TriageResultPage';
import ResolutionWorkspace from './pages/ResolutionWorkspace';
import ResolutionSummary from './pages/ResolutionSummary';

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/tickets" element={<TicketList />} />
        <Route path="/tickets/new" element={<TicketIntake />} />
        <Route path="/tickets/:id/triage" element={<TriageResultPage />} />
        <Route path="/tickets/:id/workspace" element={<ResolutionWorkspace />} />
        <Route path="/tickets/:id/summary" element={<ResolutionSummary />} />
      </Routes>
    </Layout>
  );
}
