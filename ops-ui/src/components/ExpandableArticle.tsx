import { useState } from 'react';
import { BookOpen, ChevronDown, ChevronRight } from 'lucide-react';
import type { KnowledgeArticle } from '../types';

interface Props {
  article: KnowledgeArticle;
  compact?: boolean;
}

export default function ExpandableArticle({ article, compact = false }: Props) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div
      className={`${compact ? 'p-2' : 'p-3'} bg-gray-50 rounded-lg cursor-pointer border border-transparent hover:border-blue-200 transition-colors`}
      onClick={() => setExpanded(!expanded)}
    >
      <div className="flex items-start gap-2">
        <BookOpen className={`${compact ? 'w-3.5 h-3.5' : 'w-4 h-4'} text-blue-500 mt-0.5 flex-shrink-0`} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-1.5">
            {expanded
              ? <ChevronDown className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
              : <ChevronRight className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />}
            <p className={`font-medium ${compact ? 'text-xs' : 'text-sm'} text-gray-900`}>{article.title}</p>
          </div>

          {!expanded && (
            <p className={`${compact ? 'text-xs' : 'text-xs'} text-gray-500 mt-0.5 line-clamp-2 ml-5`}>
              {article.content.replace(/[#*\n]/g, ' ').substring(0, compact ? 100 : 150)}...
            </p>
          )}

          {expanded && (
            <div className="mt-2 ml-5 p-3 bg-white border border-gray-200 rounded-lg max-h-64 overflow-y-auto">
              <p className="text-xs text-gray-700 whitespace-pre-wrap leading-relaxed">
                {article.content.replace(/[#*]/g, '')}
              </p>
            </div>
          )}

          {!compact && (
            <div className="flex items-center gap-2 mt-1 ml-5">
              <span className="text-xs text-gray-400">{article.viewCount} views</span>
              <span className="text-xs text-gray-400">•</span>
              <span className="text-xs text-green-600">{article.helpfulCount} found helpful</span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
