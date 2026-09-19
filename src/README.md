# Исходники

Этап 0 создал проекты согласно docs/PROJECT_STRUCTURE.md:

- `NexsusVKVideo.App` — WPF composition root и стартовое окно;
- `NexsusVKVideo.Core` — модели, контракты и доменные правила;
- `NexsusVKVideo.Infrastructure` — будущие реализации I/O-контрактов;
- `NexsusVKVideo.Modules` — будущие прикладные модули.

Этап 0.1 добавляет навигацию, явно подписанный demo Home и единственный WebView2-host для официального VK Video widget. Этап 0.2 добавляет SQLite-реализации Core-контрактов истории, избранного и настроек в Infrastructure, экраны Библиотеки/Избранного и Settings UI. В разделе «Поиск» можно открыть только строго проверенную публичную ссылку вида `https://vk.ru/video-{owner}_{video}`: она локально преобразуется в URL официального виджета, без API-ключа, токена или запроса к API. Каталог, live search и авторизация ещё не реализованы.
