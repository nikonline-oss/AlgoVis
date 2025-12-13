"""Точка входа в транслятор"""
import sys
import json
from src.core.translator import JAVATranslator


def main():
    if len(sys.argv) < 2:
        print("Использование: python main.py <python_file> [output_file]")
        sys.exit(1)
    
    # Читаем Python код
    python_file = sys.argv[1]
    with open(python_file, 'r', encoding='utf-8') as f:
        python_code = f.read()
    
    # Создаем транслятор
    translator = JAVATranslator(mods_dir="mods")
    
    # Настраиваем визуализацию (только сравнения и обмены)
    translator.visualization_config.enable_type("compare")
    translator.visualization_config.enable_type("swap")
    
    # Транслируем
    try:
        java_json = translator.translate(
            python_code,
            visualize_types=["compare", "swap", "condition"]
        )
        
        # Выводим результат
        if len(sys.argv) > 2:
            output_file = sys.argv[2]
            with open(output_file, 'w', encoding='utf-8') as f:
                json.dump(java_json, f, indent=2, ensure_ascii=False)
            print(f"Результат сохранен в {output_file}")
        else:
            print(json.dumps(java_json, indent=2, ensure_ascii=False))
            
    except Exception as e:
        print(f"Ошибка трансляции: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()