package com.algovis.chapi

import chapi.ast.pythonast.PythonAnalyser
import com.fasterxml.jackson.module.kotlin.jacksonObjectMapper
import io.ktor.http.*
import io.ktor.server.application.*
import io.ktor.server.request.*
import io.ktor.server.response.*
import io.ktor.server.routing.*
import org.slf4j.LoggerFactory

private val log = LoggerFactory.getLogger("ChapiRoutes")
private val mapper = jacksonObjectMapper()

fun Route.parseRoutes() {
    // Простой эндпоинт для проверки работоспособности сервиса
    get("/health") {
        call.respondText("OK", ContentType.Text.Plain)
    }

    // Основной эндпоинт для парсинга
    post("/parse") {
        // Получаем JSON из тела запроса и превращаем его в объект ParseRequest
        val req = call.receive<ParseRequest>()
        log.info("Parse request: language=${req.language}, file=${req.fileName}, size=${req.code.length}")

        try {
            // В зависимости от языка вызываем нужный анализатор Chapi
            val raw = when (req.language.lowercase()) {
                "python", "py" -> {
                    val analyser = PythonAnalyser()
                    // Вызываем парсер Chapi, передавая ему код и имя файла
                    analyser.analysis(req.code, req.fileName)
                }
                else -> throw IllegalArgumentException("Unsupported language: ${req.language}")
            }

            // Превращаем объект CodeContainer (результат Chapi) в JSON-строку
            val json = mapper.writeValueAsString(raw)
            // Отправляем JSON обратно клиенту
            call.respondText(json, ContentType.Application.Json)

        } catch (e: Exception) {
            log.error("Parse failed", e)
            // В случае ошибки возвращаем 500 и JSON с описанием ошибки
            call.respond(
                HttpStatusCode.InternalServerError,
                ErrorResponse(error = e.message ?: "Unknown error")
            )
        }
    }
}