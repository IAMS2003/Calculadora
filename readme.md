# Calculadora Básica WPF

Una calculadora de escritorio para Windows desarrollada en **C#** y **.NET** utilizando **WPF (Windows Presentation Foundation)** y el patrón arquitectónico **MVVM (Model-View-ViewModel)** de forma nativa (clásica). 

Este proyecto fue diseñado con fines de aprendizaje para dominar las bases de la programación orientada a objetos en C#, la separación de responsabilidades y las pruebas unitarias automatizadas.

---

## 🛠️ Stack Tecnológico
* **Lenguaje:** C# (C# 12/13)
* **Framework UI:** WPF (.NET 10 para Windows)
* **SDK:** .NET 10.0
* **Framework de Pruebas:** xUnit
* **Herramientas de construcción:** .NET CLI (`dotnet`) y MSBuild

---

## 📐 Arquitectura del Proyecto

El proyecto está diseñado bajo una implementación estricta y manual del patrón **MVVM clásico** (sin usar librerías externas de MVVM como CommunityToolkit para entender mejor los fundamentos).

```
┌─────────────────────────────────────────────────────────────┐
│                     VISTA (XAML)                            │
│  Views/MainWindow.xaml                                      │
│  ┌────────────┐  ┌──────────┐  ┌──────────┐               │
│  │ Display    │  │ 0-9, .   │  │ +−×÷%=C  │               │
│  └─────┬──────┘  └────┬─────┘  └─────┬────┘               │
│        │{Binding}      │{Binding}      │{Binding}           │
├────────┼───────────────┼───────────────┼────────────────────┤
│        ▼               ▼               ▼                    │
│           VIEWMODEL (C# — Estado y Comandos)                │
│  CalculatorViewModel : BaseViewModel                        │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ DisplayText, _firstOperand, _state                     │ │
│  │ NumberInputCommand  (RelayCommand<string>)              │ │
│  │ OperationCommand    (RelayCommand<string>)              │ │
│  │ EqualsCommand       (RelayCommand)                     │ │
│  │ ClearCommand        (RelayCommand)                     │ │
│  └──────────────────┬─────────────────────────────────────┘ │
│                     │ Invoca                                │
├─────────────────────┼───────────────────────────────────────┤
│                     ▼                                       │
│           MODELO (Lógica Matemática Pura — Stateless)        │
│  CalculatorModel (stateless)                                │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Add, Subtract, Multiply, Divide                        │ │
│  │ Percentage, Calculate                                  │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 1. Modelo (`CalculatorModel.cs`)
* **Libre de estado (Stateless):** No guarda ninguna información sobre las operaciones pasadas o en progreso.
* **Funciones puras:** Contiene métodos matemáticos puros (`Add`, `Subtract`, `Multiply`, `Divide`, `Percentage`). Al recibir los mismos parámetros, siempre retorna el mismo resultado, sin efectos secundarios en el sistema.

### 2. ViewModel (`CalculatorViewModel.cs`)
* **Controlador de Estado:** Contiene las variables del estado de la calculadora y la máquina de estados.
* **Comandos:** Expone propiedades `ICommand` (`NumberInputCommand`, `OperationCommand`, `EqualsCommand`, `ClearCommand`) a las que la vista XAML se enlaza.
* **BaseViewModel:** Clase abstracta de infraestructura que implementa `INotifyPropertyChanged` usando el atributo compilador `[CallerMemberName]` para notificar cambios a la vista sin errores de strings mágicos.

### 3. Vista (`MainWindow.xaml`)
* **Declarativo (XAML):** Define la UI de la calculadora en un diseño plano y oscuro (modo oscuro minimalista con colores inspirados en el diseño de iOS).
* **Code-behind Limpio:** El archivo C# asociado a la vista (`MainWindow.xaml.cs`) contiene únicamente la conexión del `DataContext` hacia el ViewModel, manteniéndose libre de lógica de negocio y capturando eventos de teclado.

---

## ✨ Características y Reglas Implementadas

1. **Máquina de Estados Sólida:** Transita ordenadamente entre estados (`Start`, `FirstOperandInput`, `OperatorSelected`, `SecondOperandInput`, `ResultDisplayed`, `Error`).
2. **Encadenamiento (Chaining) de operaciones:** Permite presionar operadores de forma consecutiva (ej: `5 + 3 * 2` realiza automáticamente `5 + 3 = 8` al presionar `*`).
3. **Porcentaje contextual:** Soporta porcentajes dependientes del operando principal (ej: `200 + 10%` calcula el 10% de 200 resultando en `220`) y porcentajes aislados (ej: `15%` de forma directa da `0.15`).
4. **Robustez Regional (`InvariantCulture`):** Diseñado para ignorar la configuración del idioma de Windows, asegurando que el separador decimal sea siempre el punto (`.`) y previniendo caídas catastróficas por diferencias en la configuración regional (como el uso de la coma decimal en países hispanohablantes).
5. **Límite en Pantalla:** Limita la entrada a un máximo de 15 dígitos en pantalla para evitar desbordamientos visuales.
6. **Manejo de errores por división entre cero:** La división por cero es interceptada a nivel de modelo mediante una excepción manual `DivideByZeroException` (ya que la CPU de forma nativa en punto flotante no la genera) y se muestra visualmente como `"Error"` inhabilitando teclas de operación hasta ser limpiado.
7. **Soporte de Teclado Completo:** Mapea automáticamente atajos físicos en la ventana utilizando eventos de túnel (`OnPreviewTextInput` y `OnPreviewKeyDown`) para que el usuario pueda escribir, borrar (Esc o Backspace) y operar utilizando su teclado estándar o numérico.

---

## 📂 Estructura de Carpetas

```
Calculadora/                          ← Raíz de la solución
├── src/
│   └── Calculadora/                  ← Proyecto WPF principal
│       ├── Calculadora.csproj
│       ├── App.xaml / App.xaml.cs    ← Arranque del ciclo de vida
│       ├── Models/
│       │   ├── CalculatorModel.cs    ← Lógica matemática pura
│       │   └── OperationType.cs      ← Enumerador de operaciones
│       ├── ViewModels/
│       │   ├── BaseViewModel.cs      ← Notificaciones de Binding
│       │   └── CalculatorViewModel.cs← Estado y comandos de la calculadora
│       ├── Views/
│       │   ├── MainWindow.xaml       ← Diseño de interfaz gráfica
│       │   └── MainWindow.xaml.cs    ← Enlace de DataContext y atajos de teclado
│       └── Commands/
│           └── RelayCommand.cs       ← Implementación nativa de ICommand y ICommand<T>
├── tests/
│   └── Calculadora.Tests/            ← Proyecto de pruebas unitarias xUnit
│       ├── Calculadora.Tests.csproj
│       ├── Models/
│       │   └── CalculatorModelTests.cs     ← Pruebas del modelo (36 casos)
│       ├── ViewModels/
│       │   ├── BaseViewModelTests.cs       ← Pruebas de notificaciones (4 casos)
│       │   └── CalculatorViewModelTests.cs ← Pruebas de la máquina de estados (18 casos)
│       └── Commands/
│           └── RelayCommandTests.cs        ← Pruebas de comandos (10 casos)
├── Calculadora.slnx                  ← Archivo de solución XML moderno (.NET 10)
├── .gitignore                        ← Archivo de exclusiones de Git
├── TODOS.md                          ← Lista de tareas diferidas
└── readme.md                         ← Este archivo de documentación
```

---

## 🚀 Cómo Compilar, Ejecutar y Probar

Puedes utilizar la consola de comandos estándar en la raíz del proyecto para realizar todas las tareas principales:

### Compilar el proyecto
```bash
dotnet build
```

### Ejecutar la aplicación
```bash
dotnet run --project src/Calculadora
```

### Ejecutar las Pruebas Unitarias
El proyecto cuenta con un total de **68 pruebas unitarias** que cubren el 100% de la lógica de cálculo y los edge cases del ViewModel. Para ejecutarlas todas y ver el reporte:
```bash
dotnet test
```
