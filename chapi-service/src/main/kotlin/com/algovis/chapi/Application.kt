package com.algovis.chapi

import io.ktor.serialization.kotlinx.json.*
import io.ktor.server.application.*
import io.ktor.server.engine.*
import io.ktor.server.netty.*
import io.ktor.server.plugins.contentnegotiation.*
import io.ktor.server.routing.*
import kotlinx.serialization.json.Json

fun main() {
    // Читаем порт и хост из переменных окружения (это удобно для настройки на сервере)
    val port = System.getenv("PORT")?.toIntOrNull() ?: 8081
    val host = System.getenv("HOST") ?: "127.0.0.1"

    // Запускаем встроенный сервер Netty
    embeddedServer(Netty, port = port, host = host) {
        // Устанавливаем плагин для автоматической работы с JSON
        install(ContentNegotiation) {
            json(Json { ignoreUnknownKeys = true })
        }
        // Регистрируем наши маршруты
        routing {
            parseRoutes()
        }
    }.start(wait = true) // wait = true означает, что программа будет работать, пока ее не остановят
}