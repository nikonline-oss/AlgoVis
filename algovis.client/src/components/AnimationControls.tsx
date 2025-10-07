import React from 'react';
import { Play, Pause, SkipForward, SkipBack, RotateCcw } from 'lucide-react';
import { Button } from './ui/button';
import { Slider } from './ui/slider';
import { useApp } from '../contexts/AppContext';

interface AnimationControlsProps {
  isPlaying: boolean;
  onPlay: () => void;
  onPause: () => void;
  onStepForward: () => void;
  onStepBackward: () => void;
  onReset: () => void;
  speed: number;
  onSpeedChange: (speed: number) => void;
  disabled?: boolean;
}

export function AnimationControls({
  isPlaying,
  onPlay,
  onPause,
  onStepForward,
  onStepBackward,
  onReset,
  speed,
  onSpeedChange,
  disabled = false,
}: AnimationControlsProps) {
  const { translations } = useApp();

  return (
    <div className="flex items-center space-x-4 p-4 bg-card border rounded-lg">
      <div className="flex items-center space-x-2">
        <Button
          variant="outline"
          size="icon"
          onClick={onStepBackward}
          disabled={disabled}
          title={translations['controls.backward']}
        >
          <SkipBack className="h-4 w-4" />
        </Button>

        <Button
          variant={isPlaying ? "secondary" : "default"}
          size="icon"
          onClick={isPlaying ? onPause : onPlay}
          disabled={disabled}
          className="bg-primary hover:bg-primary/90 text-primary-foreground"
        >
          {isPlaying ? (
            <Pause className="h-4 w-4" />
          ) : (
            <Play className="h-4 w-4" />
          )}
        </Button>

        <Button
          variant="outline"
          size="icon"
          onClick={onStepForward}
          disabled={disabled}
          title={translations['controls.forward']}
        >
          <SkipForward className="h-4 w-4" />
        </Button>

        <Button
          variant="outline"
          size="icon"
          onClick={onReset}
          disabled={disabled}
          title={translations['controls.reset']}
        >
          <RotateCcw className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex items-center space-x-3 min-w-[200px]">
        <span className="text-sm">{translations['controls.speed']}:</span>
        <Slider
          value={[speed]}
          onValueChange={(value) => onSpeedChange(value[0])}
          max={5}
          min={0.5}
          step={0.5}
          className="flex-1"
          disabled={disabled}
        />
        <span className="text-sm w-8">{speed}x</span>
      </div>
    </div>
  );
}