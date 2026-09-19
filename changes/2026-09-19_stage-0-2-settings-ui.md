# Этап 0.2 — Settings UI и очистка локальных данных

Дата: 2026-09-19

Цель: реализовать работающий экран Settings для темы, приватности и локальной очистки без серверов, телеметрии или учётных данных.

База Git: `fea8b86 feat: connect library and favorites UI`.

Компоненты: App services/resources, Core reset contract, Infrastructure scheduler, Settings ViewModel/XAML, Infrastructure tests.

Изменение: тема Dark/Light применяется через DynamicResource и сохраняется в SQLite; сохранённая тема применяется после показа оболочки на следующем запуске. Settings сразу очищает историю. Очистка cache/session профиля WebView2 ставится в очередь до следующего запуска, когда профиль ещё не открыт; SQLite-файл при этом не удаляется. Экран явно сообщает о локальном характере данных и возможной повторной авторизации у VK.

Проверки (команда/сценарий, фактический результат): `dotnet build NexsusVKVideo.sln -c Release --no-restore -m:1` — успешно, 0 warnings/0 errors; `dotnet test NexsusVKVideo.sln -c Release --no-build -m:1` — успешно, 20/20. Тест scheduler подтверждает удаление только запланированного WebView2-профиля с сохранением SQLite-файла.

Данные и совместимость: тема и marker хранятся в `%LocalAppData%\NexsusVKVideo`; кэш/сессии плеера удаляются только после явного действия пользователя и следующего запуска. Токены, пароли и cookies не записываются в SQLite или Git.

Откат (commit/порядок, резервная копия, проверка): перед `git revert <commit>` сохранить `%LocalAppData%\NexsusVKVideo\data`, затем выполнить revert и restore/build/test. Отложенная очистка WebView2 сохраняется как marker до запуска или ручного удаления marker.

Ограничения: нет системного auto-theme, очистки избранного одним действием, UI выбора папки или экспорт/импорт. Search, авторизация и VK API не добавлены. Визуальные проверки DPI/resize выполняются вручную вне текущей среды автоматизации.
