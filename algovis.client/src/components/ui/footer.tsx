import React from 'react';

interface FooterProps {
    onNavigate?: (page: string) => void;
}

export function Footer({ onNavigate }: FooterProps) {
    const currentYear = new Date().getFullYear();

    const handleNavigation = (page: string) => {
        if (onNavigate) {
            onNavigate(page);

            window.scrollTo({ top: 0, behavior: 'smooth' });
        }
    };

    return (
        <footer className="border-t border-border bg-primary/10 py-8 mt-12 relative">
            <div className="container mx-auto px-4">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">

                    <div className="space-y-3">
                        <h3 className="text-lg font-medium text-foreground">
                            Algovis
                        </h3>
                        <p className="text-sm text-muted-foreground">
                            © {currentYear} Algovis. Права пока не защищены.
                        </p>
                        
                        <p className="text-sm text-muted-foreground">
                            Почта для обратной связи:{' '}
                            <a
                                href="mailto:algo_vis@mail.ru"
                                className="text-primary hover:underline cursor-pointer"
                            >
                                algo_vis@mail.ru
                            </a>
                        </p>
                    </div>

                    <div className="space-y-3">
                        <h4 className="text-base font-medium text-foreground">
                            Дополнительно
                        </h4>
                        <div>
                            <a
                                href="https://github.com/nikonline-oss/AlgoVis"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="text-sm text-muted-foreground hover:text-primary transition-colors duration-200 cursor-pointer hover:underline"
                            >
                                GitHub
                            </a>
                        </div>
                    </div>
                </div>
                <br />
                <div className="mt-8 pt-6 border-t border-primary/20 text-center">
                    <p className="text-xs text-muted-foreground/70">
                        Визуализируем алгоритмы для вас с 2025!
                    </p>
                </div>
            </div>
        </footer>
    );
}