import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Bot, ArrowRight, BookOpen, Target, Brain, User, Tag } from 'lucide-react';
import { getTicket, getTriageResult, getArticle } from '../services/api';
import { Card } from '../components/Card';
import { PriorityBadge, StatusBadge, CategoryBadge, Badge } from '../components/Badge';
import type { Ticket, TriageResult, KnowledgeArticle } from '../types';

export default function TriageResultPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [triage, setTriage] = useState<TriageResult | null>(null);
  const [articles, setArticles] = useState<KnowledgeArticle[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      getTicket(id),
      getTriageResult(id),
    ]).then(async ([ticketRes, triageRes]) => {
      setTicket(ticketRes.data);
      setTriage(triageRes.data);
      // Fetch matched articles
      const articlePromises = triageRes.data.matchedArticleIds.map(aid => getArticle(aid).catch(() => null));
      const articleResults = await Promise.all(articlePromises);
      setArticles(articleResults.filter(r => r !== null).map(r => r!.data));
      setLoading(false);
    }).catch(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="animate-pulse"><div className="h-96 bg-gray-200 rounded-xl" /></div>;
  if (!ticket || !triage) return <p className="text-red-500">Triage result not found.</p>;

  const confidencePercent = Math.round(triage.confidence * 100);

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
            <Bot className="w-4 h-4 text-blue-500" />
            <span>AI Triage Complete</span>
            <span className="text-gray-300">•</span>
            <span>{ticket.ticketNumber}</span>
          </div>
          <h1 className="text-2xl font-bold text-gray-900">{ticket.subject}</h1>
        </div>
        <button
          onClick={() => navigate(`/tickets/${id}/workspace`)}
          className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors"
        >
          Continue to Workspace <ArrowRight className="w-4 h-4" />
        </button>
      </div>

      {/* Confidence Score */}
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-xl p-6 border border-blue-100">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center gap-2">
            <Brain className="w-5 h-5 text-blue-600" />
            <span className="font-semibold text-gray-900">AI Confidence Score</span>
          </div>
          <span className="text-2xl font-bold text-blue-600">{confidencePercent}%</span>
        </div>
        <div className="w-full bg-blue-100 rounded-full h-3">
          <div className="bg-blue-600 h-3 rounded-full transition-all" style={{ width: `${confidencePercent}%` }} />
        </div>
        <p className="text-sm text-gray-600 mt-3">{triage.reasoning}</p>
      </div>

      {/* Classification Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <Card>
          <div className="text-center">
            <Target className="w-5 h-5 text-gray-400 mx-auto mb-2" />
            <p className="text-xs text-gray-500 mb-1">Suggested Priority</p>
            <PriorityBadge priority={triage.suggestedPriority} />
          </div>
        </Card>
        <Card>
          <div className="text-center">
            <Tag className="w-5 h-5 text-gray-400 mx-auto mb-2" />
            <p className="text-xs text-gray-500 mb-1">Suggested Category</p>
            <CategoryBadge category={triage.suggestedCategory} />
          </div>
        </Card>
        <Card>
          <div className="text-center">
            <Brain className="w-5 h-5 text-gray-400 mx-auto mb-2" />
            <p className="text-xs text-gray-500 mb-1">Sentiment</p>
            <Badge variant={triage.sentiment === 'Urgent' ? 'danger' : triage.sentiment === 'Frustrated' ? 'warning' : 'default'}>
              {triage.sentiment}
            </Badge>
          </div>
        </Card>
        <Card>
          <div className="text-center">
            <User className="w-5 h-5 text-gray-400 mx-auto mb-2" />
            <p className="text-xs text-gray-500 mb-1">Suggested Assignee</p>
            <span className="text-sm font-medium text-gray-900">{triage.suggestedAssignee || 'Unassigned'}</span>
          </div>
        </Card>
      </div>

      {/* Key Phrases */}
      {triage.keyPhrases.length > 0 && (
        <Card title="Key Phrases Detected">
          <div className="flex flex-wrap gap-2">
            {triage.keyPhrases.map(phrase => (
              <span key={phrase} className="px-3 py-1 bg-gray-100 text-gray-700 rounded-full text-sm">
                {phrase}
              </span>
            ))}
          </div>
        </Card>
      )}

      {/* Matched KB Articles */}
      {articles.length > 0 && (
        <Card title="Matched Knowledge Articles" subtitle="Relevant articles found for this issue">
          <div className="space-y-3">
            {articles.map(article => (
              <div key={article.id} className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg group cursor-pointer">
                <BookOpen className="w-4 h-4 text-blue-500 mt-0.5 flex-shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-sm text-gray-900">{article.title}</p>
                  <p className="text-xs text-gray-500 mt-0.5 line-clamp-2 group-hover:hidden">
                    {article.content.replace(/[#*\n]/g, ' ').substring(0, 150)}...
                  </p>
                  {/* Full content shown on hover */}
                  <div className="hidden group-hover:block mt-1 p-2 bg-white border border-gray-200 rounded-lg max-h-48 overflow-y-auto">
                    <p className="text-xs text-gray-700 whitespace-pre-wrap">{article.content.replace(/[#*]/g, '')}</p>
                  </div>
                  <div className="flex items-center gap-2 mt-1">
                    <span className="text-xs text-gray-400">{article.viewCount} views</span>
                    <span className="text-xs text-gray-400">•</span>
                    <span className="text-xs text-green-600">{article.helpfulCount} found helpful</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </Card>
      )}

      {/* Ticket Details */}
      <Card title="Original Ticket">
        <div className="space-y-3">
          <div className="flex items-center gap-4 text-sm">
            <span className="text-gray-500">From:</span>
            <span className="font-medium">{ticket.requesterName}</span>
            <span className="text-gray-400">({ticket.requesterDepartment})</span>
          </div>
          <p className="text-sm text-gray-700 whitespace-pre-wrap bg-gray-50 p-4 rounded-lg">
            {ticket.description}
          </p>
        </div>
      </Card>
    </div>
  );
}
