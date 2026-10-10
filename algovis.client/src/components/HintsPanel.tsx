// src/components/HintsPanel.tsx
import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';

interface HintsPanelProps {
  hints: string[];
  currentStep: number;
  totalSteps: number;
  className?: string;
}

export function HintsPanel({ hints, currentStep, totalSteps, className = '' }: HintsPanelProps) {
  if (hints.length === 0) {
    return null;
  }

  const currentHint = hints[currentStep] || hints[hints.length - 1];

  return (
    <Card className={className}>
      <CardHeader className="pb-3">
        <CardTitle className="text-lg flex items-center justify-between">
          <span>Подсказки</span>
          <span className="text-sm text-muted-foreground font-normal">
            Шаг {currentStep + 1}/{totalSteps}
          </span>
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {/* Текущая подсказка */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
            <div className="flex items-start space-x-2">
              <div className="bg-blue-500 text-white rounded-full w-5 h-5 flex items-center justify-center text-xs mt-0.5 flex-shrink-0">
                i
              </div>
              <p className="text-blue-800 text-sm leading-relaxed">
                {currentHint}
              </p>
            </div>
          </div>

          {/* История подсказок (только для отладки) */}
          {hints.length > 1 && (
            <div className="border-t pt-3">
              <details className="text-sm">
                <summary className="cursor-pointer text-muted-foreground hover:text-foreground">
                  История подсказок ({hints.length})
                </summary>
                <div className="mt-2 space-y-2 max-h-32 overflow-y-auto">
                  {hints.map((hint, index) => (
                    <div
                      key={index}
                      className={`text-xs p-2 rounded border ${
                        index === currentStep
                          ? 'bg-primary/10 border-primary/20'
                          : 'bg-muted/50 border-muted'
                      }`}
                    >
                      <div className="flex items-start space-x-2">
                        <span className="text-muted-foreground font-mono flex-shrink-0">
                          {index + 1}.
                        </span>
                        <span className={index === currentStep ? 'text-primary font-medium' : 'text-muted-foreground'}>
                          {hint}
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </details>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}