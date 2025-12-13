"""Настройка пакета для установки"""
from setuptools import setup, find_packages
import os

# Читаем README для длинного описания
with open("README.md", "r", encoding="utf-8") as fh:
    long_description = fh.read()

# Читаем версию из файла
with open("VERSION", "r", encoding="utf-8") as fh:
    version = fh.read().strip()

# Читаем зависимости
with open("requirements.txt", "r", encoding="utf-8") as fh:
    requirements = [line.strip() for line in fh if line.strip() and not line.startswith("#")]

setup(
    name="java-translator",
    version=version,
    author="ЯВА Транслятор Команда",
    author_email="example@example.com",
    description="Транслятор Python кода в ЯВА (Язык Визуализации Алгоритмов)",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/yourusername/java-translator",
    project_urls={
        "Bug Tracker": "https://github.com/yourusername/java-translator/issues",
        "Documentation": "https://java-translator.readthedocs.io/",
    },
    classifiers=[
        "Development Status :: 3 - Alpha",
        "Intended Audience :: Developers",
        "Intended Audience :: Education",
        "Topic :: Software Development :: Compilers",
        "Topic :: Software Development :: Libraries :: Python Modules",
        "Topic :: Education",
        "License :: OSI Approved :: MIT License",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Operating System :: OS Independent",
    ],
    package_dir={"": "src"},
    packages=find_packages(where="src"),
    include_package_data=True,
    package_data={
        "java_translator": ["py.typed"],
    },
    python_requires=">=3.8",
    install_requires=requirements,
    extras_require={
        "dev": [
            "pytest>=7.0.0",
            "pytest-cov>=4.0.0",
            "flake8>=6.0.0",
            "black>=23.0.0",
            "mypy>=1.0.0",
            "isort>=5.12.0",
            "pre-commit>=3.0.0",
        ],
        "docs": [
            "sphinx>=7.0.0",
            "sphinx-rtd-theme>=1.3.0",
            "sphinx-autodoc-typehints>=2.0.0",
        ],
    },
    entry_points={
        "console_scripts": [
            "java-translator=java_translator.cli:main",
        ],
    },
    keywords=[
        "java",
        "algorithm",
        "visualization",
        "translator",
        "python",
        "education",
    ],
)