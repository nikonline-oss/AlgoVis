import React from 'react';
import { ArrowRight, Eye, Play, BarChart3 } from 'lucide-react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { useApp } from '../contexts/AppContext';

interface HomePageProps {
  onNavigate: (page: string) => void;
}

export function HomePage({ onNavigate }: HomePageProps) {
  const { translations } = useApp();

  const features = [
    {
      icon: Eye,
      title: translations['features.visual.title'],
      description: translations['features.visual.desc'],
    },
    {
      icon: Play,
      title: translations['features.interactive.title'],
      description: translations['features.interactive.desc'],
    },
    {
      icon: BarChart3,
      title: translations['features.analysis.title'],
      description: translations['features.analysis.desc'],
    },
  ];

  return (
    <div className="space-y-12">
      {/* Hero Section */}
      <section className="text-center space-y-6 py-12">
        <h1 className="text-4xl font-bold text-foreground">
          {translations['hero.title']}
        </h1>
        <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
          {translations['hero.subtitle']}
        </p>
        <p className="text-muted-foreground max-w-3xl mx-auto">
          {translations['hero.description']}
        </p>
        <Button
          size="lg"
          onClick={() => onNavigate('visualizer')}
          className="bg-primary hover:bg-primary/90 text-primary-foreground"
        >
          {translations['hero.start']}
          <ArrowRight className="ml-2 h-4 w-4" />
        </Button>
      </section>

      {/* Features Section */}
      <section className="space-y-8">
        <h2 className="text-3xl font-bold text-center">
          {translations['features.title']}
        </h2>
        <div className="grid md:grid-cols-3 gap-6">
          {features.map((feature, index) => (
            <Card key={index} className="text-center">
              <CardHeader>
                <div className="mx-auto w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center">
                  <feature.icon className="h-6 w-6 text-primary" />
                </div>
                <CardTitle>{feature.title}</CardTitle>
              </CardHeader>
              <CardContent>
                <CardDescription>{feature.description}</CardDescription>
              </CardContent>
            </Card>
          ))}
        </div>
      </section>

      {/* Quick Start Section */}
      <section className="bg-muted/50 rounded-lg p-8 text-center space-y-6">
        <h3 className="text-2xl font-bold">Начните прямо сейчас</h3>
        <div className="grid md:grid-cols-2 gap-4 max-w-md mx-auto">
          <Button
            variant="outline"
            onClick={() => onNavigate('visualizer')}
            className="w-full"
          >
            {translations['nav.visualizer']}
          </Button>
          <Button
            variant="outline"
            onClick={() => onNavigate('profiler')}
            className="w-full"
          >
            {translations['nav.profiler']}
          </Button>
        </div>
      </section>
    </div>
  );
}