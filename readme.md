# Calculadora Científica Avanzada, Graficadora y Estadística (WPF)

Una aplicación de escritorio profesional para Windows desarrollada en **C#** y **.NET** utilizando **WPF (Windows Presentation Foundation)** y el patrón arquitectónico **MVVM (Model-View-ViewModel)** nativo (sin frameworks de terceros).

Inspirada en el flujo de trabajo y capacidades de las calculadoras científicas **Texas Instruments (TI-Nspire CAS)** y **GeoGebra**, esta suite integra desde operaciones básicas e intermedias hasta cálculo simbólico/numérico, graficación 2D interactiva, análisis estadístico con regresión de datos y resolución de sistemas matriciales.

---

## 🛠️ Stack Tecnológico
* **Lenguaje:** C# (C# 12 / .NET 10)
* **Framework UI:** WPF (.NET 10 para Windows)
* **Motor Simbólico:** Math.NET Symbolics (F# Expr Wrapper)
* **Framework de Pruebas:** xUnit (189 pruebas unitarias automatizadas)
* **Herramientas de construcción:** .NET CLI (`dotnet`) y MSBuild

---

## 📐 Arquitectura del Proyecto

El proyecto está diseñado bajo una implementación estricta y manual del patrón **MVVM clásico**:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            VISTA (XAML + WPF)                               │
│  Views/MainWindow.xaml (Sidebar colapsable, navegación dinámica)            │
│  ├── BasicCalculatorView.xaml       ├── CalculusView.xaml                   │
│  ├── ScientificCalculatorView.xaml  ├── MatrixView.xaml                     │
│  ├── GraphPlotterView.xaml          ├── StatisticsView.xaml                 │
│  └── VirtualKeypadView.xaml         └── HistoryView.xaml                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                     VIEWMODEL (C# — Estado y Comandos)                      │
│  NavigationViewModel (Router MVVM)                                          │
│  ├── CalculatorViewModel            ├── CalculusViewModel                   │
│  ├── ScientificViewModel            ├── MatrixViewModel                     │
│  ├── GraphPlotterViewModel          ├── StatisticsViewModel                 │
│  └── HistoryViewModel               └── BaseViewModel (INotifyProperty)     │
├─────────────────────────────────────────────────────────────────────────────┤
│                  MODELO (Lógica Matemática & Algoritmos)                    │
│  ├── CalculatorModel.cs (Operaciones básicas y porcentajes)                 │
│  ├── ExpressionParser.cs & ExpressionNormalizer.cs (Parser de expresiones)  │
│  ├── SymbolicAdapter.cs (Derivación y simplificación simbólica)              │
│  ├── NumericalCalculus.cs (Simpson 1/3, Trapecio, Newton-Raphson)           │
│  ├── CoordinateTransformer.cs & FunctionAnalyzer.cs (Geometría y puntos)    │
│  ├── MatrixModel.cs & LinearSystemSolver.cs (Álgebra matricial y Gauss)     │
│  ├── StatisticsModel.cs (Estadísticas y Regresión Lineal/Exponencial)       │
│  └── HistoryService.cs & ExportService.cs (Historial y Exportación PNG/JSON)│
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## ✨ Módulos y Funcionalidades Principales

### 1. 🔢 Calculadora Básica (con Previsualización en Tiempo Real)
* **Display de Doble Línea:** Muestra la operación completa en la parte superior (`2 + 3 × 4`) y calcula una **previsualización en tiempo real** del resultado provisional (`= 14`) mientras se escribe.
* **Máquina de Estados Sólida:** Transita entre estados (`Start`, `FirstOperandInput`, `OperatorSelected`, `SecondOperandInput`, `ResultDisplayed`, `Error`).
* **Porcentaje Contextual:** Soporta porcentajes dependientes del operando (`200 + 10%` = `220`) y porcentajes directos (`15%` = `0.15`).

### 2. 🧪 Calculadora Científica & Teclado Virtual
* **Parser de Expresiones Matematicas (`ExpressionParser.cs`):** Soporta funciones trigonométricas (`sin`, `cos`, `tan`, `asin`, `acos`, `atan`), logaritmos (`ln`, `log`), exponenciales (`e`, `^`), factoriales (`!`) y constantes (`pi`, `e`).
* **Multiplicación Implícita Universal (`ExpressionNormalizer.cs`):** Interpreta automáticamente expresiones en notación humana como `3x`, `2(3)`, `2pi`, `2sin(x)` o `(2)(3)`.
* **Modos de Ángulo:** Alternador entre Grados (`DEG`) y Radianes (`RAD`).
* **Teclado Virtual Matemático (`VirtualKeypadView.xaml`):** Panel interactivo con botones rápidos para insertar símbolos y funciones.

### 3. 📈 Graficador 2D Interactivo
* **Navegación Fluida:** Control de Zoom con la rueda del ratón y arrastre del plano (*Pan*) en tiempo real.
* **Detección Automática de Puntos Notables (`FunctionAnalyzer.cs`):** Marca sobre las curvas:
  * **Raíces** ($f(x)=0$) en verde.
  * **Corte en Y** ($f(0)$) en cian.
  * **Extremos relativos (Máximos y Mínimos)** en naranja/azul.
  * **Intersecciones** entre funciones en amarillo.
* **Mira de Coordenadas:** Muestra las coordenadas $(x, y)$ del cursor en tiempo real.
* **Colores de Trazo Personalizables:** Asignación de color independiente por función (`FunctionItem.cs`) mediante una mini paleta circular.
* **Exportación PNG (`ExportService.cs`):** Botón para capturar y guardar la gráfica en imagen PNG de alta resolución.

### 4. 📐 Cálculo & Álgebra Simbólica (Estilo GeoGebra)
* **Pestaña Simbólica:** Derivación respecto a cualquier variable ($\frac{d}{dx} f(x)$), simplificación $S(x)$ y expansión de polinomios $(x+a)^n$ usando Math.NET Symbolics.
* **Integración Numérica:** Algoritmos de **Simpson (1/3)** y **Trapecio** para integrar $\int_a^b f(x) dx$ con límites definidos.
* **Resolución de Ecuaciones:** Encontrar raíces con **Newton-Raphson** y solución exacta de ecuaciones cuadráticas $ax^2 + bx + c = 0$ (soporta raíces reales y complejas).
* **Notación GeoGebra:** Botones y pestañas formateados con notación matemática elegante.

### 5. ▦ Matrices y Solvador de Sistemas $N \times N$
* **Dimensiones Editables:** Soporta matrices desde $1 \times 1$ hasta $10 \times 10$ con desplazamiento bidireccional.
* **Operaciones Matriciales:** Suma ($A+B$), Resta ($A-B$), Multiplicación ($A \times B$), Determinante ($\det(A)$), Matriz Inversa ($A^{-1}$), Transpuesta ($A^T$) y Traza ($\text{tr}(A)$).
* **Solvador de Sistemas Lineales ($AX = B$):** Algoritmo de eliminación Gauss-Jordan con pivoteo parcial (`LinearSystemSolver.cs`) para resolver sistemas de ecuaciones $N \times N$.

### 6. 📊 Estadística & Regresión de Datos
* **Editor de Puntos $(X, Y)$:** Tabla de datos interactiva con adición y eliminación de puntos.
* **Estadísticas Descriptivas:** Cálculo de medias ($\bar{x}, \bar{y}$) y desviaciones estándar ($\sigma_x, \sigma_y$).
* **Regresión de Datos:** Regresión **Lineal** ($y = mx + b$) y **Exponencial** ($y = a \cdot e^{bx}$) con coeficiente de determinación $R^2$.
* **Integración con Graficador:** Botón **"📊 Graficar Curva de Regresión en 2D"** para proyectar los puntos y la curva ajustada en el plano cartesiano.

### 7. 📜 Historial Global & Archivos de Proyecto (`.calc`)
* **`HistoryService.cs`:** Registra centralizadamente las operaciones ejecutadas con marcas de tiempo `HH:mm:ss`, distintivos por módulo y botón para **copiar expresiones al portapapeles**.
* **Archivos `.calc`:** Serialización y deserialización JSON para guardar y cargar proyectos completos.

### 8. 🎨 Interfaz de Usuario y Tema Oscuro Estilizado
* **Barra Lateral Colapsable:** Botón hamburguesa `☰` para alternar entre el menú expandido (`220px`) y compacto (`60px`) mostrando solo iconos con *tooltips*.
* **Sistema de Diseño Oscuro (`App.xaml`):** ControlTemplates personalizados para `ComboBox`, `ComboBoxItem`, `TabControl` y `TabItem` con estados de hover, resaltado de selección `#0A84FF` y bordes redondeados.

---

## 📂 Estructura del Proyecto

```
Calculadora/                          ← Raíz de la solución
├── src/
│   └── Calculadora/                  ← Proyecto WPF principal
│       ├── Calculadora.csproj
│       ├── App.xaml / App.xaml.cs    ← Sistema de diseño oscuro y arranque
│       ├── Controls/
│       │   └── GraphPlotter.cs       ← Canvas personalizado de graficación 2D
│       ├── Converters/
│       │   ├── BoolToVisibilityConverter.cs
│       │   └── NumberToGridLengthConverter.cs
│       ├── Models/
│       │   ├── CalculatorModel.cs    ├── MatrixModel.cs
│       │   ├── ExpressionParser.cs   ├── LinearSystemSolver.cs
│       │   ├── ExpressionNormalizer.cs├── StatisticsModel.cs
│       │   ├── SymbolicAdapter.cs    ├── FunctionAnalyzer.cs
│       │   ├── NumericalCalculus.cs  ├── CoordinateTransformer.cs
│       │   └── FunctionItem.cs       └── OperationType.cs
│       ├── Services/
│       │   ├── HistoryService.cs     ← Registro centralizado de historial
│       │   ├── EventAggregator.cs    ← Comunicación desacoplada entre módulos
│       │   └── ExportService.cs      ← Exportación PNG y serialización JSON
│       ├── ViewModels/
│       │   ├── BaseViewModel.cs      ├── MatrixViewModel.cs
│       │   ├── NavigationViewModel.cs├── StatisticsViewModel.cs
│       │   ├── CalculatorViewModel.cs├── HistoryViewModel.cs
│       │   ├── ScientificViewModel.cs└── GraphPlotterViewModel.cs
│       ├── Views/
│       │   ├── MainWindow.xaml / .cs ├── CalculusView.xaml / .cs
│       │   ├── BasicCalculatorView.xaml ├── MatrixView.xaml / .cs
│       │   ├── ScientificCalculatorView.xaml ├── StatisticsView.xaml / .cs
│       │   ├── GraphPlotterView.xaml ├── VirtualKeypadView.xaml / .cs
│       │   └── HistoryView.xaml / .cs
│       └── Commands/
│           └── RelayCommand.cs       ← Implementación nativa de ICommand
├── tests/
│   └── Calculadora.Tests/            ← Proyecto de pruebas unitarias xUnit (189 tests)
│       ├── Calculadora.Tests.csproj
│       ├── CalculatorModelTests.cs   ├── MatrixModelTests.cs
│       ├── CalculatorViewModelTests.cs├── NumericalCalculusTests.cs
│       ├── ExpressionParserTests.cs  ├── StatisticsModelTests.cs
│       ├── SymbolicAdapterTests.cs   ├── LinearSystemSolverTests.cs
│       ├── FunctionAnalyzerTests.cs  ├── FunctionItemTests.cs
│       └── HistoryServiceTests.cs
├── Calculadora.sln                   ← Archivo de solución .NET
└── README.md                         ← Documentación del proyecto
```

---

## 🚀 Cómo Compilar, Ejecutar y Probar

### Compilar el proyecto
```bash
dotnet build
```

### Ejecutar la aplicación
```bash
dotnet run --project src/Calculadora/Calculadora.csproj
```

### Ejecutar las Pruebas Unitarias
El proyecto cuenta con un total de **189 pruebas unitarias** que cubren la lógica de cálculo, la máquina de estados, álgebra simbólica/matricial, funciones geométricas y regresiones estadísticas. Para ejecutarlas:

```bash
dotnet test
```
