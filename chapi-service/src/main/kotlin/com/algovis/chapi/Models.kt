package com.algovis.chapi

import kotlinx.serialization.Serializable

// Модель для входящего запроса на парсинг
@Serializable
data class ParseRequest(
    val language: String = "python",
    val code: String,
    val fileName: String = "input.py"
)

// Модель для ответа в случае ошибки
@Serializable
data class ErrorResponse(
    val success: Boolean = false,
    val error: String
)