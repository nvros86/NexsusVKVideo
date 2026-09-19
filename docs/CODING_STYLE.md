# C# и тестирование
Nullable включён; implicit usings допустимы; отступ 4 пробела, UTF-8, конечный newline.
PascalCase для типов/методов, camelCase параметров, _camelCase private fields; Async суффикс у Task-методов.
ViewModel содержит presentation state и команды; code-behind — только специфическое UI-поведение.
Не использовать async void вне событий, .Result/.Wait для I/O, catch без обработки, fire-and-forget без наблюдения ошибок.
CancellationToken обязателен для отменяемых операций. IDisposable/IAsyncDisposable реализуется при владении ресурсом.
Логирование структурированное, без токенов, cookies и URL с секретами.
Пакеты добавлять обоснованно, версии фиксировать, не использовать floating production dependencies.

Тесты поведения: отмена поиска, latest-wins, дедупликация избранного, миграции, provider error mapping и границы модулей.
Unit tests не обращаются к VK; integration tests opt-in, без реальных учётных данных в CI.
UI smoke: загрузка, пусто, ошибка, keyboard focus, resize и DPI. Проверки не должны только повторять строки реализации.
