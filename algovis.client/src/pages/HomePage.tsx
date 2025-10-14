import React from 'react';
import { ArrowRight, Eye, Play, BarChart3 } from 'lucide-react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { useApp } from '../contexts/AppContext';
import Particles from '../components/ui/Particles';
import SpotlightCard from '../components/ui/SpotlightCard';
import ShinyText from '../components/ui/ShinyText';
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
    <div className="relative min-h-screen">
      {/* Particles Background */}
      <div
        className="fixed inset-0 z-10"
        style={{
          width: '100vw',
          height: '100vh',
          position: 'fixed',
          top: 0,
          left: 0
        }}
      >
        <Particles
          particleColors={['#00ff00']}
          particleCount={200}
          particleSpread={10}
          speed={0.1}
          particleBaseSize={200}
          moveParticlesOnHover={false}
          alphaParticles={true}
          disableRotation={false}
        />
      </div>

      {/* Content */}
      <div className="relative z-10 space-y-12">
        {/* Hero Section */}
        <section className="text-center space-y-6 py-12">
          <h1 className="text-4xl font-bold text-foreground">
            {translations['hero.title']}
          </h1>
          <p className="text-xl text-muted-foreground max-w-2xl mx-auto">
            <ShinyText
              text={translations['hero.subtitle']}
              disabled={false}
              speed={3}
              className='custom-class'
            />
          </p>
          <p className="text-muted-foreground max-w-3xl mx-auto">
            <ShinyText
              text={translations['hero.description']}
              disabled={false}
              speed={3}
              className='custom-class'
            />
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
              <Card key={index} className="text-center border-0">
                <SpotlightCard className="custom-spotlight-card" spotlightColor="rgba(79, 255, 170, 0.44)">

                  <CardHeader>
                    <div className="mx-auto w-12 h-12 bg-primary/10 rounded-lg flex items-center justify-center">
                      <feature.icon className="h-6 w-6 text-primary" />
                    </div>
                    <CardTitle>{feature.title}</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <CardDescription>{feature.description}</CardDescription>
                  </CardContent>
                </SpotlightCard>
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
    </div>
  );
}