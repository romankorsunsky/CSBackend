import yfinance as yf

tickerName = "AAPL"

ticker = yf.Ticker(tickerName)

news = ticker.get_news(10,tab="press releases")

for n in news:
    content = n['content']
    title = content["title"]
    summary = content["summary"]
    print(f"title = {title}\n summary = {summary}")