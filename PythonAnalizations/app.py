"""REST API для транслятора Python → ЯВА"""
import json
import traceback
from flask import Flask, request, jsonify
from flask_cors import CORS
from python_java_translator.src.core.translator import JAVATranslator
from python_java_translator.src.core.validator import JAVAValidator

app = Flask(__name__)
CORS(app)  # Разрешаем CORS для всех доменов

# Инициализируем транслятор и валидатор
translator = JAVATranslator()
validator = JAVAValidator()

@app.route('/')
def index():
    """Главная страница API"""
    return jsonify({
        "api": "ЯВА (Язык Визуализации Алгоритмов) - Транслятор Python → ЯВА",
        "version": "1.0.0",
        "endpoints": {
            "/translate": "POST - Трансляция Python кода в ЯВА JSON",
            "/validate": "POST - Валидация ЯВА JSON",
            "/health": "GET - Проверка работоспособности API"
        },
        "documentation": "https://github.com/yourusername/java-translator"
    })

@app.route('/health', methods=['GET'])
def health_check():
    """Проверка работоспособности API"""
    return jsonify({
        "status": "healthy",
        "service": "java-translator-api",
        "version": "1.0.0"
    })

@app.route('/translate', methods=['POST'])
def translate():
    """
    Трансляция Python кода в ЯВА JSON
    
    Параметры запроса (JSON):
    - code: Python код для трансляции (обязательно)
    - visualize_types: список типов шагов для визуализации (опционально)
    - mods_dir: путь к директории с пользовательскими модами (опционально)
    - validate: валидировать ли результат (по умолчанию: true)
    
    Пример запроса:
    {
        "code": "def bubble_sort():\n    n = struct.len\n    for i in range(n):\n        for j in range(n - 1):\n            if struct.values[j] > struct.values[j + 1]:\n                temp = struct.values[j]\n                struct.values[j] = struct.values[j + 1]\n                struct.values[j + 1] = temp",
        "visualize_types": ["compare", "swap", "condition"],
        "mods_dir": "mods",
        "validate": true
    }
    """
    try:
        # Получаем данные из запроса
        data = request.get_json()
        
        if not data:
            return jsonify({
                "error": "Нет данных в запросе",
                "success": False
            }), 400
        
        # Проверяем наличие кода
        if 'code' not in data:
            return jsonify({
                "error": "Отсутствует поле 'code' в запросе",
                "success": False
            }), 400
        
        code = data['code']
        
        # Извлекаем дополнительные параметры
        visualize_types = data.get('visualize_types', [])
        mods_dir = data.get('mods_dir', 'mods')
        validate_result = data.get('validate', True)
        
        # Создаем транслятор с указанной директорией модов
        translator = JAVATranslator(mods_dir=mods_dir)
        
        # Выполняем трансляцию
        java_json = translator.translate(
            python_code=code,
            visualize_types=visualize_types
        )
        
        # Валидируем результат если нужно
        validation_result = None
        if validate_result:
            validation_result = validator.validate_json(json.dumps(java_json, ensure_ascii=False))
            
            if not validation_result['valid']:
                # Возвращаем результат с предупреждениями
                return jsonify({
                    "success": True,
                    "java": java_json,
                    "validation": validation_result,
                    "warning": "Результат содержит ошибки валидации"
                }), 200
        
        return jsonify({
            "success": True,
            "java": java_json,
            "validation": validation_result
        }), 200
        
    except Exception as e:
        # Логируем ошибку
        app.logger.error(f"Ошибка трансляции: {str(e)}")
        app.logger.error(traceback.format_exc())
        
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc() if app.debug else None
        }), 500

@app.route('/validate', methods=['POST'])
def validate():
    """
    Валидация ЯВА JSON
    
    Параметры запроса (JSON):
    - java: ЯВА JSON для валидации (обязательно)
    
    Пример запроса:
    {
        "java": {
            "name": "BubbleSort",
            "description": "Сортировка пузырьком",
            "structureType": "array",
            "variables": [...],
            "steps": [...]
        }
    }
    """
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({
                "error": "Нет данных в запросе",
                "success": False
            }), 400
        
        if 'java' not in data:
            return jsonify({
                "error": "Отсутствует поле 'java' в запросе",
                "success": False
            }), 400
        
        java_data = data['java']
        
        # Проверяем, является ли java_data строкой или словарем
        if isinstance(java_data, dict):
            java_json = json.dumps(java_data, ensure_ascii=False)
        elif isinstance(java_data, str):
            java_json = java_data
        else:
            return jsonify({
                "error": "Поле 'java' должно быть строкой или объектом JSON",
                "success": False
            }), 400
        
        # Выполняем валидацию
        validation_result = validator.validate_json(java_json)
        
        return jsonify({
            "success": True,
            "validation": validation_result
        }), 200
        
    except json.JSONDecodeError as e:
        return jsonify({
            "success": False,
            "error": f"Ошибка декодирования JSON: {str(e)}"
        }), 400
    except Exception as e:
        app.logger.error(f"Ошибка валидации: {str(e)}")
        app.logger.error(traceback.format_exc())
        
        return jsonify({
            "success": False,
            "error": str(e),
            "traceback": traceback.format_exc() if app.debug else None
        }), 500

@app.route('/examples', methods=['GET'])
def get_examples():
    """
    Получение примеров алгоритмов
    
    Параметры запроса (query parameters):
    - category: категория примеров (sorting, searching, custom)
    """
    try:
        category = request.args.get('category', 'sorting')
        
        examples = {
            "sorting": {
                "bubble_sort": {
                    "name": "bubble_sort",
                    "description": "Сортировка пузырьком",
                    "code": """def bubble_sort():
    n = struct.len
    
    for i in range(n):
        for j in range(n - i - 1):
            if struct.values[j] > struct.values[j + 1]:
                temp = struct.values[j]
                struct.values[j] = struct.values[j + 1]
                struct.values[j + 1] = temp""",
                    "visualize_types": ["compare", "swap", "condition"]
                },
                "selection_sort": {
                    "name": "selection_sort",
                    "description": "Сортировка выбором",
                    "code": """def selection_sort():
    n = struct.len
    
    for i in range(n):
        min_idx = i
        for j in range(i + 1, n):
            if struct.values[j] < struct.values[min_idx]:
                min_idx = j
        
        if min_idx != i:
            temp = struct.values[i]
            struct.values[i] = struct.values[min_idx]
            struct.values[min_idx] = temp""",
                    "visualize_types": ["compare", "swap", "condition"]
                }
            },
            "searching": {
                "linear_search": {
                    "name": "linear_search",
                    "description": "Линейный поиск",
                    "code": """def linear_search(target):
    n = struct.len
    found = False
    position = -1
    
    for i in range(n):
        if struct.values[i] == target:
            found = True
            position = i
            break
    
    return found, position""",
                    "visualize_types": ["condition", "assign"]
                },
                "binary_search": {
                    "name": "binary_search",
                    "description": "Бинарный поиск (только для отсортированных массивов)",
                    "code": """def binary_search(target):
    left = 0
    right = struct.len - 1
    found = False
    position = -1
    
    while left <= right:
        mid = (left + right) // 2
        
        if struct.values[mid] == target:
            found = True
            position = mid
            break
        elif struct.values[mid] < target:
            left = mid + 1
        else:
            right = mid - 1
    
    return found, position""",
                    "visualize_types": ["condition", "assign"]
                }
            },
            "custom": {
                "find_max": {
                    "name": "find_max",
                    "description": "Поиск максимального элемента",
                    "code": """def find_max():
    if struct.len == 0:
        return None
    
    max_value = struct.values[0]
    max_index = 0
    
    for i in range(1, struct.len):
        if struct.values[i] > max_value:
            max_value = struct.values[i]
            max_index = i
    
    return max_value, max_index""",
                    "visualize_types": ["condition", "assign"]
                },
                "reverse_array": {
                    "name": "reverse_array",
                    "description": "Разворот массива",
                    "code": """def reverse_array():
    left = 0
    right = struct.len - 1
    
    while left < right:
        temp = struct.values[left]
        struct.values[left] = struct.values[right]
        struct.values[right] = temp
        
        left += 1
        right -= 1""",
                    "visualize_types": ["swap", "condition"]
                }
            }
        }
        
        if category in examples:
            return jsonify({
                "success": True,
                "category": category,
                "examples": examples[category]
            }), 200
        else:
            return jsonify({
                "success": False,
                "error": f"Категория '{category}' не найдена. Доступные категории: {', '.join(examples.keys())}"
            }), 404
            
    except Exception as e:
        app.logger.error(f"Ошибка получения примеров: {str(e)}")
        
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/mods', methods=['GET'])
def list_mods():
    """Получение списка загруженных модов"""
    try:
        mods_info = []
        
        for mod in translator.mods:
            mods_info.append({
                "name": mod.name,
                "priority": mod.priority,
                "dependencies": mod.get_dependencies() if hasattr(mod, 'get_dependencies') else []
            })
        
        return jsonify({
            "success": True,
            "mods_dir": translator.mods_dir,
            "mods_count": len(mods_info),
            "mods": mods_info
        }), 200
        
    except Exception as e:
        app.logger.error(f"Ошибка получения списка модов: {str(e)}")
        
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.route('/batch-translate', methods=['POST'])
def batch_translate():
    """
    Пакетная трансляция нескольких Python файлов
    
    Параметры запроса (JSON):
    - files: список объектов с полями 'name' и 'code'
    - options: общие опции для всех файлов (опционально)
    
    Пример запроса:
    {
        "files": [
            {
                "name": "bubble_sort.py",
                "code": "def bubble_sort(): ..."
            },
            {
                "name": "linear_search.py", 
                "code": "def linear_search(target): ..."
            }
        ],
        "options": {
            "visualize_types": ["compare", "swap"],
            "validate": true
        }
    }
    """
    try:
        data = request.get_json()
        
        if not data:
            return jsonify({
                "error": "Нет данных в запросе",
                "success": False
            }), 400
        
        if 'files' not in data:
            return jsonify({
                "error": "Отсутствует поле 'files' в запросе",
                "success": False
            }), 400
        
        files = data['files']
        options = data.get('options', {})
        
        if not isinstance(files, list):
            return jsonify({
                "error": "Поле 'files' должно быть массивом",
                "success": False
            }), 400
        
        results = []
        
        for file_info in files:
            if not isinstance(file_info, dict) or 'name' not in file_info or 'code' not in file_info:
                results.append({
                    "name": file_info.get('name', 'unknown'),
                    "success": False,
                    "error": "Файл должен содержать поля 'name' и 'code'"
                })
                continue
            
            try:
                # Транслируем каждый файл
                file_translator = JAVATranslator(mods_dir=options.get('mods_dir', 'mods'))
                
                java_json = file_translator.translate(
                    python_code=file_info['code'],
                    visualize_types=options.get('visualize_types', [])
                )
                
                # Валидируем если нужно
                validation_result = None
                if options.get('validate', True):
                    validation_result = validator.validate_json(json.dumps(java_json, ensure_ascii=False))
                
                results.append({
                    "name": file_info['name'],
                    "success": True,
                    "java": java_json,
                    "validation": validation_result
                })
                
            except Exception as e:
                results.append({
                    "name": file_info['name'],
                    "success": False,
                    "error": str(e)
                })
        
        # Подсчитываем статистику
        successful = sum(1 for r in results if r['success'])
        failed = len(results) - successful
        
        return jsonify({
            "success": True,
            "summary": {
                "total": len(results),
                "successful": successful,
                "failed": failed
            },
            "results": results
        }), 200
        
    except Exception as e:
        app.logger.error(f"Ошибка пакетной трансляции: {str(e)}")
        app.logger.error(traceback.format_exc())
        
        return jsonify({
            "success": False,
            "error": str(e)
        }), 500

@app.errorhandler(404)
def not_found(error):
    """Обработчик 404 ошибок"""
    return jsonify({
        "success": False,
        "error": f"Endpoint {request.path} не найден"
    }), 404

@app.errorhandler(405)
def method_not_allowed(error):
    """Обработчик 405 ошибок"""
    return jsonify({
        "success": False,
        "error": f"Метод {request.method} не разрешен для {request.path}"
    }), 405

if __name__ == '__main__':
    # Конфигурация приложения
    app.config['JSON_AS_ASCII'] = False  # Для поддержки кириллицы
    app.config['MAX_CONTENT_LENGTH'] = 16 * 1024 * 1024  # Ограничение 16MB
    
    # Запуск сервера
    print("ЯВА Транслятор API запущен")
    print("Доступные эндпоинты:")
    print("  GET  /                    - Информация об API")
    print("  GET  /health              - Проверка работоспособности")
    print("  POST /translate           - Трансляция Python кода")
    print("  POST /validate            - Валидация ЯВА JSON")
    print("  GET  /examples            - Примеры алгоритмов")
    print("  GET  /mods                - Список загруженных модов")
    print("  POST /batch-translate     - Пакетная трансляция")
    
    app.run(host='0.0.0.0', port=5000, debug=True)