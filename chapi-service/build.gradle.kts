plugins {
    kotlin("jvm") version "2.0.20"
    kotlin("plugin.serialization") version "2.0.20"
    application
}

repositories {
    mavenCentral()
    maven { url = uri("https://jitpack.io") }
}

dependencies {
    implementation("io.ktor:ktor-server-core:3.0.0")
    implementation("io.ktor:ktor-server-netty:3.0.0")
    implementation("io.ktor:ktor-server-content-negotiation:3.0.0")
    implementation("io.ktor:ktor-serialization-kotlinx-json:3.0.0")

    // --- Библиотека Chapi ---
    // Актуальные модули: domain (модели) и ast-python (парсер Python).
    // Версия 2.5.2 — последняя на момент написания.
    implementation("com.phodal.chapi:chapi-domain:2.5.2")
    implementation("com.phodal.chapi:chapi-ast-python:2.5.2")

    implementation("com.fasterxml.jackson.module:jackson-module-kotlin:2.17.2")
    implementation("ch.qos.logback:logback-classic:1.5.6")
}

application {
    mainClass.set("com.algovis.chapi.ApplicationKt")
}

kotlin {
    jvmToolchain(21)
}