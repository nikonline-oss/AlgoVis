import React, { useState } from 'react';
import { AppProvider } from './contexts/AppContext';
import { Header } from './components/Header';
import { HomePage } from './pages/HomePage';
import { VisualizerPage } from './pages/VisualizerPage';
import { ProfilerPage } from './pages/ProfilerPage';
import { CodeAnalyzerPage } from './pages/CodeAnalyzerPage';
import { HelpPage } from './pages/HelpPage';

export default function App() {
  const [currentPage, setCurrentPage] = useState('home');

  const renderPage = () => {
    switch (currentPage) {
      case 'home':
        return <HomePage onNavigate={setCurrentPage} />;
      case 'visualizer':
        return <VisualizerPage />;
      case 'profiler':
        return <ProfilerPage onNavigate={setCurrentPage} />;
      case 'analyzer':
        return <CodeAnalyzerPage />;
      case 'help':
        return <HelpPage />;
      default:
        return <HomePage onNavigate={setCurrentPage} />;
    }
  };

  return (
    <AppProvider>
      <div className="min-h-screen bg-background">
        <Header currentPage={currentPage} onPageChange={setCurrentPage} />
        <main className="container mx-auto px-4 py-8">
          {renderPage()}
        </main>
      </div>
    </AppProvider>
  );
}