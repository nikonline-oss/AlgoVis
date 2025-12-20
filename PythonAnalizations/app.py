"""
REST API для транслятора Python → ЯВА
Упрощенная версия с одним маршрутом /translate
"""
import json
import os
import sys
import traceback
from flask import Flask, request, jsonify
from flask_cors import CORS

# Добавляем путь к модулям транслятора
sys.path.append(os.path.join(os.path.dirname(__file__), 'python_java_translator'))

try:
    # Импортируем наш сервис трансляции
    from yava_translator_service import YAVATranslatorService
except ImportError as e:
    print(f"Ошибка импорта: {e}")
    print("Убедитесь, что модули транслятора находятся в правильной директории")
    raise


app = Flask(__name__)
CORS(app)  # Разрешаем CORS для всех доменов

# Инициализируем транслятор
translator = YAVATranslatorService()

@app.route('/')
def index():
    """Главная страница API"""
    return jsonify({
        "api": "ЯВА (Язык Визуализации Алгоритмов) - Транслятор Python → ЯВА",
        "version": "2.0.0",
        "endpoint": "POST /translate - Трансляция Python кода в ЯВА JSON",
        "documentation": "Использует новый транслятор на основе YAVATranslatorService"
    })

@app.route('/health', methods=['GET'])
def health_check():
    """Проверка работоспособности API"""
    return jsonify({
        "status": "healthy",
        "service": "java-translator-api",
        "version": "2.0.0",
        "translator_ready": True
    })

@app.route('/translate', methods=['POST'])
def translate():
    """
    Трансляция Python кода в ЯВА JSON
    
    Параметры запроса (JSON):
    - code: Python код для трансляции (обязательно)
    - program_name: Название алгоритма (опционально)
    - description: Описание алгоритма (опционально)
    - structure_type: Тип структуры данных (опционально, по умолчанию "array")
    - validate: валидировать ли результат (опционально, по умолчанию true)
    
    Пример запроса:
    {
        "code": "arr = [5, 2, 8, 1, 9, 3]\nn = len(arr)\nfor i in range(n):\n    for j in range(0, n - i - 1):\n        if arr[j] > arr[j + 1]:\n            temp = arr[j]\n            arr[j] = arr[j + 1]\n            arr[j + 1] = temp",
        "program_name": "Bubble Sort",
        "description": "Сортировка пузырьком с визуализацией",
        "structure_type": "array",
        "validate": true
    }
    """
    try:
        # Получаем данные из запроса
        data = request.get_json()
        
        if not data:
            return jsonify({
                "error": "Нет данных в запросе. Отправьте JSON с полем 'code'",
                "success": False
            }), 400
        
        # Проверяем наличие кода
        if 'code' not in data:
            return jsonify({
                "error": "Отсутствует обязательное поле 'code' в запросе",
                "success": False
            }), 400
        
        code = data['code']
        
        # Извлекаем дополнительные параметры
        program_name = data.get('program_name', 'Algorithm')
        description = data.get('description', 'Сгенерированный алгоритм')
        structure_type = data.get('structure_type', 'array')
        validate_result = data.get('validate', True)
        
        # Выполняем трансляцию
        try:
            yava_json = translator.translate_from_python(
                python_code=code,
                program_name=program_name,
                description=description,
                structure_type=structure_type
            )
        except Exception as e:
            return jsonify({
                "success": False,
                "error": f"Ошибка трансляции: {str(e)}",
                "suggestion": "Проверьте синтаксис Python кода"
            }), 400
        
        # Валидируем результат если нужно
        validation_result = None
        if validate_result:
            is_valid, errors = translator.validate_yava(yava_json)
            validation_result = {
                "valid": is_valid,
                "errors": errors,
                "warnings": [] if is_valid else ["Обнаружены ошибки в структуре ЯВА JSON"]
            }
            
            # Если есть ошибки валидации, все равно возвращаем результат с предупреждением
            if not is_valid:
                return jsonify({
                    "success": True,
                    "yava": yava_json,
                    "validation": validation_result,
                    "warning": "Результат содержит ошибки валидации, но был возвращен"
                }), 200
        
        # Получаем статистику
        stats = translator.get_statistics(yava_json)
        
        return jsonify({
            "success": True,
            "yava": yava_json,
            "validation": validation_result,
            "statistics": stats,
            "message": "Трансляция успешно выполнена"
        }), 200
        
    except json.JSONDecodeError:
        return jsonify({
            "success": False,
            "error": "Неверный формат JSON в запросе"
        }), 400
        
    except Exception as e:
        # Логируем ошибку
        app.logger.error(f"Ошибка в API: {str(e)}")
        app.logger.error(traceback.format_exc())
        
        return jsonify({
            "success": False,
            "error": f"Внутренняя ошибка сервера: {str(e)}",
            "traceback": traceback.format_exc() if app.debug else None
        }), 500

@app.errorhandler(404)
def not_found(error):
    """Обработчик 404 ошибок"""
    return jsonify({
        "success": False,
        "error": f"Маршрут {request.path} не найден. Используйте POST /translate",
        "available_routes": {
            "GET /": "Информация об API",
            "GET /health": "Проверка работоспособности",
            "POST /translate": "Трансляция Python кода в ЯВА JSON"
        }
    }), 404

@app.errorhandler(405)
def method_not_allowed(error):
    """Обработчик 405 ошибок"""
    return jsonify({
        "success": False,
        "error": f"Метод {request.method} не разрешен для {request.path}"
    }), 405

def run_api_server(host='0.0.0.0', port=5001, debug=False):
    """Запуск сервера API"""
    print("🚀 ЯВА Транслятор API запущен")
    print(f"📡 Адрес: http://{host}:{port}")
    print("\n📋 Доступные эндпоинты:")
    print("  GET  /                    - Информация об API")
    print("  GET  /health              - Проверка работоспособности")
    print("  POST /translate           - Трансляция Python кода в ЯВА JSON")
    print("\n📝 Пример использования curl:")
    print(f'  curl -X POST http://{host}:{port}/translate \\')
    print('    -H "Content-Type: application/json" \\')
    print('    -d \'{"code": "arr = [5, 2, 8, 1, 9, 3]\\nfor i in range(len(arr)):\\n    print(i)", "program_name": "Test"}\'')
    
    app.run(host=host, port=port, debug=debug)

if __name__ == '__main__':
    # Конфигурация приложения
    app.config['JSON_AS_ASCII'] = False  # Для поддержки кириллицы
    app.config['MAX_CONTENT_LENGTH'] = 32 * 1024 * 1024  # Ограничение 32MB
    
    # Запуск сервера
    run_api_server(debug=True)