import React from 'react';
import { Moon, Sun, Globe } from 'lucide-react';
import { Button } from './ui/button';
import { useApp } from '../contexts/AppContext';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';

interface HeaderProps {
  currentPage: string;
  onPageChange: (page: string) => void;
}

export function Header({ currentPage, onPageChange }: HeaderProps) {
  const { language, setLanguage, theme, setTheme, translations } = useApp();

  const pages = [
    { id: 'home', label: translations['nav.home'] },
    { id: 'visualizer', label: translations['nav.visualizer'] },
    { id: 'profiler', label: translations['nav.profiler'] },
    { id: 'analyzer', label: translations['nav.analyzer'] },
    { id: 'help', label: translations['nav.help'] },
  ];

  return (
    <header className="border-b bg-card/50 backdrop-blur-sm sticky top-0 z-50">
      <div className="container mx-auto px-4 py-4 flex items-center justify-between">
        <div className="flex items-center space-x-8">
          <h1 className="text-xl font-medium text-primary">
            {translations['site.title']}
          </h1>
          <nav className="hidden md:flex space-x-6">
            {pages.map((page) => (
              <Button
                key={page.id}
                variant={currentPage === page.id ? 'default' : 'ghost'}
                onClick={() => onPageChange(page.id)}
                className={currentPage === page.id ? 'bg-primary/10 text-primary' : ''}
              >
                {page.label}
              </Button>
            ))}
          </nav>
        </div>

        <div className="flex items-center space-x-2">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon">
                <Globe className="h-5 w-5" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              <DropdownMenuItem
                onClick={() => setLanguage('ru')}
                className={language === 'ru' ? 'bg-primary/10' : ''}
              >
                Русский
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => setLanguage('en')}
                className={language === 'en' ? 'bg-primary/10' : ''}
              >
                English
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>

          <Button
            variant="ghost"
            size="icon"
            onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}
          >
            {theme === 'light' ? <Moon className="h-5 w-5" /> : <Sun className="h-5 w-5" />}
          </Button>
        </div>
      </div>
    </header>
  );
}