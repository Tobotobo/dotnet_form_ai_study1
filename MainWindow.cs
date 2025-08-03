class MainWindow : Window
{
    BehaviorSubject<State> _state = new(new State(
        User: new User(
            Furigana: "トーキョー　タロウ",
            Name: "東京　太郎",
            BirthDate: "2020/01/01",
            PhoneNumber: "01-2222-3333",
            Email: "tokyo.taro@example.com",
            PostalCode: "123-4567",
            Address: "東京都XX区YY 1-2-3",
            Notes: "※未成年です。"
        ),
        SystemPrompt: @"## 役割
あなたは優秀なエンジニアです。以下の `User データ` に対して、以下の `チェック内容` を確認してください。
チャック内容に反する項目が見つかった場合は以下の `出力指示` に従って指摘してください。

## 出力指示
* どの項目の何が問題か簡潔に `・` による箇条書きで出力してください。
* 問題が見つからなかった場合は、`問題は見つかりませんでした。` と出力してください。
* 上記以外は出力しないでください。

## チェック内容
* フリガナ
  * 全角カタカナであること
  * 姓名の間は全角スペースで区切ること
  * 氏名に対して妥当なフリガナであること
* 氏名
  * 姓名の間は全角スペースで区切ること
  * フリガナに対して妥当な氏名であること
* 生年月日
  * フォーマットが `yyyy/mm/dd` であること
  * 前 0 が付いていること
  * 半角数字と `/` のみであること
* 電話番号
  * 半角数字と記号は `-` のみであること
  * 電話番号として妥当であること
* メールアドレス
  * 半角英数字と記号は `@` `_` `-` `.` のみであること
  * メールアドレスとして妥当であること
* 郵便番号
  * 半角数字と記号は `-` のみであること
  * 郵便番号として妥当であること
* 住所
  * 住所として妥当であること
* 備考
  * 年齢が 20 歳未満の場合は、未成年であることを記載すること

## User データ",
        Result: null,
        ApiKey: Environment.GetEnvironmentVariable("GEMINI_API_KEY")
    ));

    public MainWindow()
    {
        this.Title("dotnet_form_ai_study1").Width(800).Height(600)
        .Content(MainPanel());
    }

    Panel MainPanel() => Grid()
        .ColumnDefinitions(new("*, 4, *")).Children(
            LeftPanel().Column(0),
            GridSplitter().Column(1).ResizeDirectionColumns().Background(Brushes.Gray),
            RightPanel().Column(2)
        );

    Panel LeftPanel() => Grid()
        .RowDefinitions(new("42, 42, 42, 42, 42, 42, 42, *")).ColumnDefinitions(new("Auto, *"))
        .Var(0, out var row)
        .Var((string content) => Label()
            .Content(content).Column(0).Row(row).HorizontalAlignmentRight().VerticalAlignmentCenter()
            , out var l)
        .Var((Func<State, string?> selecter, Func<State, string, State> updater) => TextBox()
            .Column(1).Row(row++).Margin(5)
            .Text(_state.Select(selecter))
            .OnText((_, o) => o.Subscribe(text => UpdateState(updater(_state.Value, text))))
            , out var tb)
        .Children(
            l("フリガナ"),       tb(state => state.User.Furigana, (state, text) => UpdateFurigana(state, text)),
            l("氏名"),           tb(state => state.User.Name, (state, text) => UpdateName(state, text)),
            l("生年月日"),       tb(state => state.User.BirthDate, (state, text) => UpdateBirthDate(state, text)),
            l("電話番号"),       tb(state => state.User.PhoneNumber, (state, text) => UpdatePhoneNumber(state, text)),
            l("メールアドレス"), tb(state => state.User.Email, (state, text) => UpdateEmail(state, text)),
            l("郵便番号"),       tb(state => state.User.PostalCode, (state, text) => UpdatePostalCode(state, text)),
            l("住所"),           tb(state => state.User.Address, (state, text) => UpdateAddress(state, text)),
            l("備考").VerticalAlignmentTop().Margin(0, 5),
                                 tb(state => state.User.Notes, (state, text) => UpdateNotes(state, text)).AcceptsReturn(true)
        );

    Panel RightPanel() => Grid()
        .RowDefinitions(new("*, 4, *")).ColumnDefinitions(new("*"))
        .Children(
            RightTopPanel().Row(0),
            GridSplitter().Row(1).ResizeDirectionRows().Background(Brushes.Gray),
            RightBottomPanel().Row(2)
        );

    Panel RightTopPanel() => Grid()
        .RowDefinitions(new("Auto, Auto, *, Auto")).ColumnDefinitions(new("Auto, *"))
        .Children(
            Label()
                .Content("Gemini API Key").Column(0).Row(0).HorizontalAlignmentRight().VerticalAlignmentCenter()
                .Margin(5, 0, 0, 0),
            TextBox()
                .Column(1).Row(0).Margin(5, 5, 5, 0).PasswordChar('*')
                .Text(_state.Select(state => state.ApiKey))
                .OnText((_, o) => o.Subscribe(text => UpdateState(UpdateApiKey(_state.Value, text)))),
            Label()
                .Content("システムプロンプト").Row(1).Margin(5, 5, 5, 0).ColumnSpan(2),
            TextBox().Column(0).Row(2).Margin(5, 0, 5, 5).AcceptsReturn(true).ColumnSpan(2)
                .Text(_state.Select(state => state.SystemPrompt))
                .OnText((_, o) => o.Subscribe(text => UpdateState(UpdateSystemPrompt(_state.Value, text)))),
            Button()
                .Content("実行").Row(3).Margin(5, 0, 5, 5).HorizontalAlignmentStretch().TextAlignmentCenter()
                .OnClick((_, o) => o.Subscribe(async _ => await ExecAIAsyncCommand())).ColumnSpan(2)
        );

    Panel RightBottomPanel() => Grid()
        .RowDefinitions(new("Auto, *")).ColumnDefinitions(new("*"))
        .Children(
            Label().Content("実行結果").Row(0).Margin(5, 5, 5, 0),
            TextBox().Column(0).Row(1).Margin(5, 0, 5, 5).AcceptsReturn(true).IsReadOnly(true)
                .Text(_state.Select(state => state.Result))
        );

    public async Task ExecAIAsyncCommand()
    {
        UpdateState(UpdateResult(_state.Value, "実行中..."));
        string result;
        try
        {
            var state = _state.Value;
            var prompt = $"{state.SystemPrompt}\n{state.User}";
            var apiKey = state.ApiKey ?? throw new Exception("Gemini API Key が設定されていません。");
            result = await Gemini.InvokePromptAsync(prompt, apiKey);
        }
        catch (Exception ex)
        {
            result = ex.ToString();
        }
        UpdateState(UpdateResult(_state.Value, result));
    }

    public void UpdateState(State state) => _state.OnNext(state);
    public static State UpdateFurigana(State state, string text) => state with { User = state.User with { Furigana = text } };
    public static State UpdateName(State state, string text) => state with { User = state.User with { Name = text } };
    public static State UpdateBirthDate(State state, string text) => state with { User = state.User with { BirthDate = text } };
    public static State UpdatePhoneNumber(State state, string text) => state with { User = state.User with { PhoneNumber = text } };
    public static State UpdateEmail(State state, string text) => state with { User = state.User with { Email = text } };
    public static State UpdatePostalCode(State state, string text) => state with { User = state.User with { PostalCode = text } };
    public static State UpdateAddress(State state, string text) => state with { User = state.User with { Address = text } };
    public static State UpdateNotes(State state, string text) => state with { User = state.User with { Notes = text } };
    public static State UpdateSystemPrompt(State state, string text) => state with { SystemPrompt = text };
    public static State UpdateResult(State state, string text) => state with { Result = text };
    public static State UpdateApiKey(State state, string text) => state with { ApiKey = text };

    public record State(
        User User,
        string? SystemPrompt,
        string? Result,
        string? ApiKey
    );
}