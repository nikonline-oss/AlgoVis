// hooks/useBackend.ts
import { useState, useCallback } from 'react';
import { apiService } from '../services/apiService';

export function useBackend() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const callApi = useCallback(async <T>(
    apiCall: () => Promise<T>,
    successMessage?: string
  ): Promise<T | null> => {
    setLoading(true);
    setError(null);
    
    try {
      const result = await apiCall();
      if (successMessage) {
        console.log(successMessage);
      }
      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      setError(errorMessage);
      console.error('API Error:', err);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  const profileAlgorithm = useCallback((algorithm: string, data: any, config: any) => {
    return callApi(
      () => apiService.profileAlgorithm(algorithm, data, config),
      `Алгоритм ${algorithm} проанализирован`
    );
  }, [callApi]);

  const analyzeCode = useCallback((code: string) => {
    return callApi(
      () => apiService.analyzeCode(code),
      'Код проанализирован'
    );
  }, [callApi]);

  const switchToRealBackend = useCallback((url: string) => {
    apiService.setBackendUrl(url);
  }, []);

  const switchToMockBackend = useCallback(() => {
    apiService.useMockBackend();
  }, []);

  return {
    loading,
    error,
    profileAlgorithm,
    analyzeCode,
    switchToRealBackend,
    switchToMockBackend,
    clearError: () => setError(null)
  };
}