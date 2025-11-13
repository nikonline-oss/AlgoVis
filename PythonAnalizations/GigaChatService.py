import requests
import uuid
import time
import threading 
from typing import Optional, List, Dict, Any
import json
import re


class GigaChatService:
    def __init__(self):
        self.auth_key = "MDE5YTRhNzEtYWEyYy03MjM4LWExMjUtNTZmNTIwNDg1MTRhOjAzZTU1NDNkLWQ1MGQtNDVhMy1iYWU5LWE3ODkxY2Y4MzVkNA=="
        self.scope = "GIGACHAT_API_PERS"
        self.token: Optional[str] = None
        self.token_expiry: Optional[float] = None
        self.is_refreshing = False
        self.refresh_queue: List[Dict[str, Any]] = []
        self.lock = threading.Lock()

    def get_token(self, force_refresh: bool = False) -> str:
        """Получение токена доступа с кешированием и очередью запросов"""
        
        # Если токен есть и не истек, и не запрошено принудительное обновление
        if (self.token and self.token_expiry and 
            time.time() < self.token_expiry and not force_refresh):
            return self.token

        # Если уже идет обновление токена, добавляем запрос в очередь
        if self.is_refreshing:
            return self._wait_for_token_refresh()

        self.is_refreshing = True

        try:
            print("Requesting new GigaChat token...", {
                "clientId": "019a4a71-aa2c-7238-a125-56f52048514a",
                "rqUid": str(uuid.uuid4()),
            })

            url = "https://ngw.devices.sberbank.ru:9443/api/v2/oauth"
            headers = {
                "Content-Type": "application/x-www-form-urlencoded",
                "Accept": "application/json",
                "RqUID": str(uuid.uuid4()),
                "Authorization": f"Basic {self.auth_key}",
            }
            data = {"scope": self.scope}

            response = requests.post(
                url, 
                headers=headers, 
                data=data, 
                verify=False,
                timeout=10
            )
            response.raise_for_status()
            
            token_data = response.json()
            if not token_data.get("access_token"):
                raise ValueError("No access token in response")

            self.token = token_data["access_token"]
            # Токен действует 30 минут, устанавливаем expiry на 25 минут для запаса
            self.token_expiry = time.time() + 25 * 60

            print(
                "GigaChat token obtained successfully, expires at:",
                time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(self.token_expiry))
            )

            # Разрешаем все ожидающие запросы
            self._resolve_pending_requests(self.token)
            return self.token

        except Exception as error:
            print(f"GigaChat token error: {error}")
            # Отклоняем все ожидающие запросы
            self._reject_pending_requests(error)
            raise Exception(f"Failed to get GigaChat token: {error}")
        finally:
            self.is_refreshing = False

    def _wait_for_token_refresh(self) -> str:
        """Ожидание обновления токена другими запросами"""
        future = {"event": threading.Event(), "token": None, "error": None}
        with self.lock:
            self.refresh_queue.append(future)
        
        future["event"].wait()
        
        if future["error"]:
            raise future["error"]
        return future["token"]

    def _resolve_pending_requests(self, token: str):
        """Разрешение всех ожидающих запросов"""
        with self.lock:
            for future in self.refresh_queue:
                future["token"] = token
                future["event"].set()
            self.refresh_queue.clear()

    def _reject_pending_requests(self, error: Exception):
        """Отклонение всех ожидающих запросов"""
        with self.lock:
            for future in self.refresh_queue:
                future["error"] = error
                future["event"].set()
            self.refresh_queue.clear()

    def chat_completion(self, messages: List[Dict[str, str]], **kwargs) -> str:
        """Основной метод для отправки запросов к GigaChat API"""
        try:
            print("Sending request to GigaChat API...")

            token = self.get_token()
            url = "https://gigachat.devices.sberbank.ru/api/v1/chat/completions"
            headers = {
                "Content-Type": "application/json",
                "Accept": "application/json",
                "Authorization": f"Bearer {token}",
            }
            
            data = {
                "model": "GigaChat",
                "messages": messages,
                "stream": False,
                "repetition_penalty": 1,
                "temperature": 0.7,
                "max_tokens": 1000,
                **kwargs
            }

            response = requests.post(
                url,
                headers=headers,
                json=data,
                verify=False,
                timeout=30
            )
            response.raise_for_status()

            print("GigaChat API response received")
            result = response.json()
            return result["choices"][0]["message"]["content"]

        except requests.exceptions.HTTPError as error:
            if error.response.status_code == 401:
                print("Token expired, forcing refresh...")
                # Токен истек, принудительно обновляем
                self.token = None
                self.token_expiry = None
                raise Exception("Token expired, please retry")
            
            print(f"GigaChat API HTTP error: {error}")
            raise Exception(f"GigaChat API request failed: {error}")
        except Exception as error:
            print(f"GigaChat API error: {error}")
            raise Exception(f"GigaChat API request failed: {error}")

    def send_message(self, message: str, **kwargs) -> str:
        """Упрощенный метод для отправки одного сообщения"""
        messages = [{"role": "user", "content": message}]
        return self.chat_completion(messages, **kwargs)

    def refresh_token(self) -> str:
        """Принудительное обновление токена"""
        self.token = None
        self.token_expiry = None
        return self.get_token(force_refresh=True)

    def get_token_status(self) -> Dict[str, Any]:
        """Get token status"""
        return {
            "has_token": bool(self.token),
            "expires_at": time.strftime("%Y-%m-%d %H:%M:%S", time.localtime(self.token_expiry)) 
                         if self.token_expiry else None,
            "is_expired": self.token_expiry is None or time.time() >= self.token_expiry,
            "time_until_expiry": self.token_expiry - time.time() 
                               if self.token_expiry else None,
        }



# Создаем глобальный экземпляр сервиса
giga_chat_service = GigaChatService()


# Упрощенный интерфейс для быстрого использования
def ask_gigachat(question: str) -> str:
    systemPromt = f"""
    
    """



    """Простая функция для быстрых запросов"""
    return giga_chat_service.send_message(question)




# Пример использования
if __name__ == "__main__":
    try:
        # Простой запрос
        response = ask_gigachat("Привет! Напиши короткое тестовое сообщение.")
        print("Ответ:", response)
        
        # Проверка статуса токена
        status = giga_chat_service.get_token_status()
        print("Статус токена:", status)
        
        
    except Exception as e:
        print(f"Ошибка: {e}")