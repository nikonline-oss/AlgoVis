import React, { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Button } from './ui/button';
import { ChevronLeft, ChevronRight, GraduationCap, SkipForward } from 'lucide-react';

interface TutorialStep {
  title: string;
  content: string;
  action?: () => void;
}

interface TutorialModeProps {
  dataStructure: string;
  algorithm: string;
  onComplete: () => void;
}

export function TutorialMode({ dataStructure, algorithm, onComplete }: TutorialModeProps) {
  const [currentStep, setCurrentStep] = useState(0);
  const [isActive, setIsActive] = useState(true);

  const getTutorialSteps = (): TutorialStep[] => {
    const steps: TutorialStep[] = [
      {
        title: "Добро пожаловать в обучающий режим!",
        content: "В этом режиме мы поможем вам освоить работу с визуализатором алгоритмов. Следуйте инструкциям для эффективного обучения."
      },
      {
        title: "Шаг 1: Выбор структуры данных",
        content: `Вы выбрали ${dataStructure === 'array' ? 'массив' : 
          dataStructure === 'tree' ? 'дерево' : 
          dataStructure === 'graph' ? 'граф' : 
          dataStructure === 'list' ? 'список' : 
          dataStructure === 'stack' ? 'стек' : 'очередь'}. Это отличный выбор для начала изучения алгоритмов!`
      },
      {
        title: "Шаг 2: Изучение алгоритма",
        content: `Алгоритм "${algorithm}" поможет вам понять основы ${dataStructure === 'array' ? 'сортировки данных' : 
          dataStructure === 'tree' ? 'обхода деревьев' : 
          dataStructure === 'graph' ? 'работы с графами' : 
          'структур данных'}.`
      },
      {
        title: "Шаг 3: Генерация данных",
        content: "Используйте кнопку 'Генерировать данные' для создания случайного набора. Попробуйте разные размеры для сравнения."
      },
      {
        title: "Шаг 4: Запуск анимации",
        content: "Нажмите кнопку Play для запуска анимации. Используйте паузу и пошаговое выполнение для детального изучения."
      },
      {
        title: "Шаг 5: Анализ результатов",
        content: "Смотрите статистику выполнения и сравнивайте разные алгоритмы. Обращайте внимание на количество операций."
      }
    ];

    return steps;
  };

  const steps = getTutorialSteps();

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    } else {
      handleComplete();
    }
  };

  const handlePrev = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleComplete = () => {
    setIsActive(false);
    onComplete();
  };

  if (!isActive) return null;

  return (
    <Card className="border-yellow-200 dark:border-yellow-800 bg-yellow-50 dark:bg-yellow-900/20">
      <CardHeader className="pb-3">
        <div className="flex justify-between items-center">
          <div className="flex items-center">
            <GraduationCap className="w-5 h-5 mr-2 text-yellow-600 dark:text-yellow-400" />
            <CardTitle className="text-lg">🎓 Обучающий режим</CardTitle>
          </div>
          <div className="text-sm text-muted-foreground">
            Шаг {currentStep + 1} из {steps.length}
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="mb-4">
          <h3 className="font-semibold text-lg mb-2">{steps[currentStep].title}</h3>
          <p className="text-sm">{steps[currentStep].content}</p>
        </div>

        <div className="flex justify-between items-center">
          <div className="flex space-x-2">
            <Button 
              variant="outline" 
              size="sm" 
              onClick={handlePrev}
              disabled={currentStep === 0}
            >
              <ChevronLeft className="w-4 h-4 mr-1" />
              Назад
            </Button>
            
            <Button 
              variant="outline" 
              size="sm" 
              onClick={handleComplete}
            >
              <SkipForward className="w-4 h-4 mr-1" />
              Пропустить обучение
            </Button>
          </div>

          <Button 
            size="sm"
            onClick={handleNext}
          >
            {currentStep < steps.length - 1 ? (
              <>
                Далее
                <ChevronRight className="w-4 h-4 ml-1" />
              </>
            ) : (
              'Завершить обучение'
            )}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}