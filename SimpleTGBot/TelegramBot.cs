namespace SimpleTGBot;

using System;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

public class TelegramBot
{
    // Токен TG-бота. Можно получить у @BotFather
    private const string BotToken = "6842594116:AAGD_vgs8z205-m9Hi50lSk_S8j09L2pNqc";

    /// <summary>
    /// Инициализирует и обеспечивает работу бота до нажатия клавиши Esc
    /// </summary>
    public async Task Run()
    {
        // Если вам нужно хранить какие-то данные во время работы бота (массив информации, логи бота,
        // историю сообщений для каждого пользователя), то это всё надо инициализировать в этом методе.
        // TODO: Инициализация необходимых полей

        // Инициализируем наш клиент, передавая ему токен.
        var botClient = new TelegramBotClient(BotToken);

        // Служебные вещи для организации правильной работы с потоками
        using CancellationTokenSource cts = new CancellationTokenSource();

        // Разрешённые события, которые будет получать и обрабатывать наш бот.
        // Будем получать только сообщения. При желании можно поработать с другими событиями.
        ReceiverOptions receiverOptions = new ReceiverOptions()
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        // Привязываем все обработчики и начинаем принимать сообщения для бота
        botClient.StartReceiving(
            updateHandler: OnMessageReceived,
            pollingErrorHandler: OnErrorOccured,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
        );

        // Проверяем что токен верный и получаем информацию о боте
        var me = await botClient.GetMeAsync(cancellationToken: cts.Token);
        Console.WriteLine($"Бот @{me.Username} запущен.\nДля остановки нажмите клавишу Esc...");

        // Ждём, пока будет нажата клавиша Esc, тогда завершаем работу бота
        while (Console.ReadKey().Key != ConsoleKey.Escape) { }

        // Отправляем запрос для остановки работы клиента.
        cts.Cancel();
    }

    /// <summary>
    /// Обработчик события получения сообщения.
    /// </summary>
    /// <param name="botClient">Клиент, который получил сообщение</param>
    /// <param name="update">Событие, произошедшее в чате. Новое сообщение, голос в опросе, исключение из чата и т. д.</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    async Task OnMessageReceived(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Работаем только с сообщениями. Остальные события игнорируем
        var message = update.Message;
        if (message is null)
        {
            return;
        }
        // Будем обрабатывать только текстовые сообщения.
        // При желании можно обрабатывать стикеры, фото, голосовые и т. д.
        //
        // Обратите внимание на использованную конструкцию. Она эквивалентна проверке на null, приведённой выше.
        // Подробнее об этом синтаксисе: https://medium.com/@mattkenefick/snippets-in-c-more-ways-to-check-for-null-4eb735594c09
        if (message.Text is not { } messageText)
        {
            return;
        }

        // Получаем ID чата, в которое пришло сообщение. Полезно, чтобы отличать пользователей друг от друга.
        var chatId = message.Chat.Id;
        string path = "logs.txt";
        using (var fs = new FileStream(path, FileMode.Create))
        using (var sw = new StreamWriter(fs))
        {
            sw.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");
        }


        // Печатаем на консоль факт получения сообщения
        Console.WriteLine($"Получено сообщение в чате {chatId}: '{messageText}'");

        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
        {
            new KeyboardButton[] { "/help", "/genres" },

            })
        {
            ResizeKeyboard = true,
        };

        string[] _genres = { "кодомо", "сёнэн", "сёдзё", "сэйнэн", "исторический", "комедия", "киберпанк", "апокалиптика", "приключения" };

        Dictionary<int, string> genres = new Dictionary<int, string>
            {
               {1, "Кодомо" },
               {2, "Сёнэн"},
               {3, "Сёдзё" },
               {4, "Сэйнэн"  },
               {5, "Исторический" },
               {6, "Комедия" },
               {7, "Киберпанк" },
               {8, "Апокалиптика" },
               {9, "Приключения" },
            };


        async void Genres()
        {
            string str = "Выберете жанр, который вам по душе:\n";

            foreach (var kv in genres)
            {
                str += $"{kv.Key}. {kv.Value}\n";
            }

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: str + "Если вам что-то непонятно, то используйте команду /help",
                cancellationToken: cancellationToken
                );
        }


        List<string> animes = new List<string> { "покемон", "часы ёкаев", "астробой", "истории охотников за монстрами: поездка",
            "охотник х охотник", "стальной алхимик: братство", "ван-пис", "магическая битва", "атака титанов",
            "корзинка фруктов", "очень приятно бог", "нана", "не сдавайся",
            "бармен: бокал бога", "басня", "ведьма и чудовище", "танец мечей: пылающий хоннодзи", "ворон не выбирает господина", "манускрипт ниндзя", "асура",
            "крутой учитель онидзука", "необъятный океан", "папаши-дружбаны", "этот замечательный мир!", "призрак в доспехах: синдром одиночки", "киберпанк: бегущие по краю",
            "последний серафим", "кабанэри железной крепости", "триган", "эрго прокси", "созданный в бездне",
            "новые врата", "провожающая в последний путь фрирен", "унесённые призраками", "ходячий замок"};


        string[] variants = { "привет", "ку", "хай", "hello", "hi", "/start" };

        //Приветствие
        if (variants.Contains(message.Text.ToLower()))
        {
            Message sentMessage1 = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Здравствуйте! \nЯ бот, который поможет вам с выбором аниме.",
                cancellationToken: cancellationToken
                );

            Genres();
        }

        //применение команды /genres
        else if (message.Text.ToLower().Equals("/genres"))
        {
            Genres();
        }

        //Вывод аниме по жанру
        else if (_genres.Contains(message.Text.ToLower()))
        {
            string str = "";
            string path1 = $"{message.Text.ToLower()}\\" + message.Text.ToLower() + ".txt";
            Console.WriteLine(path1);
            using (var sr = new StreamReader(path1))
            {
                while (!sr.EndOfStream)
                {
                    str += $"{sr.ReadLine()}\n";
                }
            }

            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: str,
                cancellationToken: cancellationToken
                );

            Message sentMessage1 = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Если вас заинтересовало какое-то аниме из списка, то введите его название(без года выпуска), чтобы увидеть кадр из него и получить его описание",
                cancellationToken: cancellationToken
                );
        }

        //Команда /help
        else if (message.Text.ToLower().Equals("/help"))
        {
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Если вы хотите снова посмотреть список жанров, то используйте команду /help",
                cancellationToken: cancellationToken
                );

            Message sentMessage1 = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Если вам незнаком какой-либо из жанров, то напишите сообщение следующего формата:\n" +
                "жанр <название неизвестного жанра>",
                cancellationToken: cancellationToken
                );
        }
        else if (animes.Contains(message.Text.ToLower()))
        {
            GetAnimeAsync(animes.IndexOf(message.Text.ToLower()));
        }
        else if (message.Text.ToLower().Contains("жанр"))
        {
            GenresDescription(message);
        }
        else
        {
            Message sentMessageDefault = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Я вас не понимаю",
                cancellationToken: cancellationToken
                );
            Message message1 = await botClient.SendStickerAsync(
                chatId: chatId,
                sticker: InputFile.FromUri("https://sl.combot.org/kitayan/webp/18xf09f98ad.webp"),
                cancellationToken: cancellationToken);
        }



        async Task GetAnimeAsync(int i)
        {
            Message sentPhoto = await botClient.SendPhotoAsync(
                    chatId: chatId,
                    photo: InputFile.FromUri(AnimeDiscriptions.urls[i]),
                    caption: AnimeDiscriptions._discriptions[i],
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);
        }

        async void GenresDescription(Message message)
        {
            try
            {
                string pattern = @"[Жж]анр\s*([а-яё]+)";
                string name = Regex.Match(message.Text.ToLower(), pattern).Groups[1].Value;
                List<string> genresDiscription = new List<string>();
                using (var sr = new StreamReader("Genres.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        genresDiscription.Add(sr.ReadLine());
                    }
                }
                string answer = genresDiscription.Where(s => s.Split().First().Equals(name)).First();
                Message sentMessage2 = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Жанр " + answer,
                    cancellationToken: cancellationToken
                    );
            }
            catch (Exception ex)
            {
                Message sentMessage2 = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Неверно введено название жанра.",
                    cancellationToken: cancellationToken
                    );
            }
        }
    }

    /// <summary>
    /// Обработчик исключений, возникших при работе бота
    /// </summary>
    /// <param name="botClient">Клиент, для которого возникло исключение</param>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="cancellationToken">Служебный токен для работы с многопоточностью</param>
    /// <returns></returns>
    Task OnErrorOccured(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // В зависимости от типа исключения печатаем различные сообщения об ошибке
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",

            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);

        // Завершаем работу
        return Task.CompletedTask;
    }
}