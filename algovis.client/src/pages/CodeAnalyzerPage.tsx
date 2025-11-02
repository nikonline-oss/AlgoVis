import React, { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Textarea } from '../components/ui/textarea';
import { useApp } from '../contexts/AppContext';
import { PlayCircle, Code, AlertCircle, CheckCircle, Loader2 } from 'lucide-react';
import { Alert, AlertDescription } from '../components/ui/alert';

export function CodeAnalyzerPage() {
  const { translations } = useApp();
  const [code, setCode] = useState('');
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [analysisResult, setAnalysisResult] = useState<{
    success: boolean;
    message: string;
    details?: any;
  } | null>(null);

  const handleAnalyze = async () => {
    if (!code.trim()) {
      setAnalysisResult({
        success: false,
        message: translations['analyzer.emptyCode'] || 'Пожалуйста, введите код для анализа',
      });
      return;
    }

    setIsAnalyzing(true);
    setAnalysisResult(null);

    // Симуляция вызова бэкенда
    // В реальном приложении здесь будет вызов API
    setTimeout(() => {
      // Пример результата анализа
      setAnalysisResult({
        success: true,
        message: translations['analyzer.success'] || 'Анализ завершен успешно',
        details: {
          complexity: 'O(n²)',
          warnings: 2,
          suggestions: 3,
        },
      });
      setIsAnalyzing(false);
    }, 2000);

    // Реальный вызов API будет выглядеть так:
    // try {
    //   const response = await fetch('/api/analyze', {
    //     method: 'POST',
    //     headers: { 'Content-Type': 'application/json' },
    //     body: JSON.stringify({ code, language: 'python' }),
    //   });
    //   const data = await response.json();
    //   setAnalysisResult(data);
    // } catch (error) {
    //   setAnalysisResult({
    //     success: false,
    //     message: translations['analyzer.error'] || 'Ошибка при анализе кода',
    //   });
    // } finally {
    //   setIsAnalyzing(false);
    // }
  };

  const handleClear = () => {
    setCode('');
    setAnalysisResult(null);
  };

  const exampleCode = `def bubble_sort(arr):
    n = len(arr)
    for i in range(n):
        for j in range(0, n-i-1):
            if arr[j] > arr[j+1]:
                arr[j], arr[j+1] = arr[j+1], arr[j]
    return arr

# Пример использования
numbers = [64, 34, 25, 12, 22, 11, 90]
sorted_numbers = bubble_sort(numbers)
print(sorted_numbers)`;

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      <div className="space-y-2">
        <h1 className="text-3xl">{translations['analyzer.title']}</h1>
        <p className="text-muted-foreground">
          {translations['analyzer.description']}
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Левая панель - ввод кода */}
        <Card className="border-2 border-primary/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Code className="w-5 h-5 text-primary" />
              {translations['analyzer.codeInput']}
            </CardTitle>
            <CardDescription>
              {translations['analyzer.codeInputDesc']}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="relative">
              <Textarea
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder={translations['analyzer.placeholder'] || exampleCode}
                className="min-h-[400px] font-mono text-sm resize-none"
                disabled={isAnalyzing}
              />
              <div className="absolute bottom-2 right-2 text-xs text-muted-foreground bg-background px-2 py-1 rounded">
                {code.split('\n').length} {translations['analyzer.lines'] || 'строк'}
              </div>
            </div>

            <div className="flex gap-2">
              <Button
                onClick={handleAnalyze}
                disabled={isAnalyzing || !code.trim()}
                className="flex-1"
                size="lg"
              >
                {isAnalyzing ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    {translations['analyzer.analyzing']}
                  </>
                ) : (
                  <>
                    <PlayCircle className="w-4 h-4 mr-2" />
                    {translations['analyzer.analyze']}
                  </>
                )}
              </Button>
              <Button
                onClick={handleClear}
                variant="outline"
                disabled={isAnalyzing}
                size="lg"
              >
                {translations['analyzer.clear']}
              </Button>
            </div>

            <Button
              onClick={() => setCode(exampleCode)}
              variant="ghost"
              size="sm"
              className="w-full"
              disabled={isAnalyzing}
            >
              {translations['analyzer.loadExample']}
            </Button>
          </CardContent>
        </Card>

        {/* Правая панель - результаты */}
        <Card className="border-2 border-primary/20">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              {analysisResult?.success ? (
                <CheckCircle className="w-5 h-5 text-green-500" />
              ) : (
                <AlertCircle className="w-5 h-5 text-amber-500" />
              )}
              {translations['analyzer.results']}
            </CardTitle>
            <CardDescription>
              {translations['analyzer.resultsDesc']}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {!analysisResult && !isAnalyzing && (
              <div className="min-h-[400px] flex items-center justify-center text-center text-muted-foreground">
                <div className="space-y-2">
                  <Code className="w-12 h-12 mx-auto opacity-50" />
                  <p>{translations['analyzer.noResults']}</p>
                </div>
              </div>
            )}

            {isAnalyzing && (
              <div className="min-h-[400px] flex items-center justify-center">
                <div className="text-center space-y-4">
                  <Loader2 className="w-12 h-12 mx-auto animate-spin text-primary" />
                  <p className="text-muted-foreground">
                    {translations['analyzer.analyzingMessage']}
                  </p>
                </div>
              </div>
            )}

            {analysisResult && (
              <div className="space-y-4">
                <Alert variant={analysisResult.success ? "default" : "destructive"}>
                  <AlertDescription className="flex items-center gap-2">
                    {analysisResult.success ? (
                      <CheckCircle className="w-4 h-4" />
                    ) : (
                      <AlertCircle className="w-4 h-4" />
                    )}
                    {analysisResult.message}
                  </AlertDescription>
                </Alert>

                {analysisResult.details && (
                  <div className="space-y-3 pt-4">
                    <Card className="bg-muted/30">
                      <CardContent className="pt-6">
                        <div className="space-y-3">
                          <div className="flex justify-between items-center">
                            <span className="text-sm">{translations['analyzer.complexity']}</span>
                            <span className="font-mono px-3 py-1 bg-primary/10 text-primary rounded">
                              {analysisResult.details.complexity}
                            </span>
                          </div>
                          <div className="flex justify-between items-center">
                            <span className="text-sm">{translations['analyzer.warnings']}</span>
                            <span className="font-mono px-3 py-1 bg-amber-500/10 text-amber-600 dark:text-amber-400 rounded">
                              {analysisResult.details.warnings}
                            </span>
                          </div>
                          <div className="flex justify-between items-center">
                            <span className="text-sm">{translations['analyzer.suggestions']}</span>
                            <span className="font-mono px-3 py-1 bg-green-500/10 text-green-600 dark:text-green-400 rounded">
                              {analysisResult.details.suggestions}
                            </span>
                          </div>
                        </div>
                      </CardContent>
                    </Card>

                    <div className="text-xs text-muted-foreground text-center pt-2">
                      {translations['analyzer.note']}
                    </div>
                  </div>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Информационная секция */}
      <Card className="bg-muted/30">
        <CardHeader>
          <CardTitle className="text-lg">{translations['analyzer.infoTitle']}</CardTitle>
        </CardHeader>
        <CardContent>
          <ul className="space-y-2 text-sm text-muted-foreground">
            <li className="flex items-start gap-2">
              <CheckCircle className="w-4 h-4 mt-0.5 text-primary" />
              <span>{translations['analyzer.info1']}</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle className="w-4 h-4 mt-0.5 text-primary" />
              <span>{translations['analyzer.info2']}</span>
            </li>
            <li className="flex items-start gap-2">
              <CheckCircle className="w-4 h-4 mt-0.5 text-primary" />
              <span>{translations['analyzer.info3']}</span>
            </li>
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
