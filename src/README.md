# Исходники

Этап 0 создал проекты согласно docs/PROJECT_STRUCTURE.md:

- `NexsusVKVideo.App` — WPF composition root и стартовое окно;
- `NexsusVKVideo.Core` — модели, контракты и доменные правила;
- `NexsusVKVideo.Infrastructure` — будущие реализации I/O-контрактов;
- `NexsusVKVideo.Modules` — будущие прикладные модули.

Этап 0.1 добавляет навигацию, явно подписанный demo Home и единственный WebView2-host для официального VK Video widget. Адаптер каталога VK, поиск, авторизация, хранилища и реальные данные ещё не реализованы.
