import requests, json, os, urllib.request

class varClass:
    def __init__(self):
        self.episodesToDownload = []
        self.username = ""
        self.password = ""
        self.outdir = ""
        self.selectedResolution = 0

vC = varClass()

session = requests.session()
session.get("https://anime-on-demand.de")
auth_token = str(session.get("https://www.anime-on-demand.de/users/sign_in").content).split("name=\"authenticity_token\" value=\"")[1].split("\"")[0]
print(f"Auth Token: {auth_token}")
if os.path.isfile("credentials.cfg"):
    creds = open("credentials.cfg", "r").readlines()
    vC.username = creds[0][:-1]
    vC.password = creds[1]
else:
    vC.username = input("Username: ")
    vC.password = input("Password: ")
    if input("Remember me (y/n): ").lower() == "y":
    	open("credentials.cfg", "w").write(f"{vC.username}\n{vC.password}")
login = {"utf8": "✓",
         "authenticity_token": auth_token,
         "user[login]": vC.username,
         "user[password]": vC.password,
         "user[remember_me]": "1",
         "commit": "Einloggen"}
result = str(session.post("https://www.anime-on-demand.de/users/sign_in", login).content)
if vC.username in result:
    print(f"Eingeloggt (als {vC.username})!")
else:
    print(result)
    input()
applied = str(session.get("https://www.anime-on-demand.de/html5beta").content)
if "TEST ANMELDEN" in applied.upper():
    session.get("https://www.anime-on-demand.de/html5apply")
    print("Der HTML5 Modus für Videos wurde aktiviert.")
animes = str(session.get("https://www.anime-on-demand.de/myanimes").content).split("animebox-title\">")
animeList = [(animes[i].split("<")[0], animes[i].split("<a href=\"/anime/")[1].split("\"")[0]) for i in range(1, len(animes))]

def joinChunks(episodeName, episodeDest):
    with open(os.path.join(episodeDest, f"{episodeName}.ts"), "wb") as f0:
        for f1 in range(len(os.listdir("temp"))):
            f0.write(open(f"temp/{f1}.ts", "rb").read())
            print(f1)
        f0.close()

def downloadEpisode(link, csrfToken, referer, output):
    print(f"CSRF: {csrfToken}")
    response = session.get(f"https://anime-on-demand.de{link}",
                           headers={"UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36",
                                    "Accept": "application/json, text/javascript, */*; q=0.01",
                                    "X-CSRF-Token": csrfToken,
                                    "X-Requested-With": "XMLHttpRequest",
                                    "Host": "www.anime-on-demand.de",
                                    "Referer": referer,
                                    "ContentType": "application/json"}).content
    obj = json.loads(response)
    playlist = obj["playlist"][0]["sources"][0]["file"]
    baseURL = playlist[:(playlist.rfind("/")-len(playlist))] + "/"
    chunkListURL = ""
    playlistlines = str(session.get(playlist).content).split("\\n")
    availResolutions = [(playlistlines[i].split(",RESOLUTION=")[1], playlistlines[i+1]) for i in range(len(playlistlines)) if "EXT-X-STREAM-INF:BANDWIDTH=" in playlistlines[i]]
    for r in range(len(availResolutions)):
    	print(f"{r}: {availResolutions[r][0]}")
    vC.selectedResolution = int(input("Auflösung > "))
    chunkListURL = baseURL + availResolutions[vC.selectedResolution][1]
    print("Lade Chunk-Liste herunter...")
    fullChunkList = str(session.get(chunkListURL).content).split("\\n")
    print("Fertig!\nLade Chunks herunter...")
    chunkList = [baseURL + a for a in fullChunkList if "media_" in a]
    if not os.path.isdir("temp"):
        os.mkdir("temp")
    for a in range(len(chunkList)):
        urllib.request.urlretrieve(chunkList[a], f"temp/{a}.ts")
        print(f"{a+1} von {len(chunkList)} Chunks fertig...")
    print("Fertig!")
    joinChunks(output, vC.outdir)
    for file in os.listdir("temp"):
        os.remove(f"temp/{file}")
    os.rmdir("temp")

while True:
    print("==========Anime Liste==========")
    for i in range(len(animeList)):
        k = animeList[i]
        print(f"{i}: {k[0]} ({k[1]})")
    curr = animeList[int(input("Anime > "))]
    print(f"=========={curr[0]}==========")
    episodeString = str(session.get(f"https://anime-on-demand.de/anime/{curr[1]}",
                                    headers={"UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36",
                                             "Accept": "application/json, text/javascript, */*; q=0.01",
                                             "X-Requested-With": "XMLHttpRequest",
                                             "Host": "www.anime-on-demand.de",
                                             "ContentType": "application/json"}).content)
    episodeList = episodeString.split("class=\"episodebox-title\" title=\"")
    isFilm = False
    if len(episodeList) == 1:
        episodeList = episodeString.split("class=\"besides-box")
        isFilm = True
    CSRF = episodeString.split("<meta name=\"csrf-token\" content=\"")[1].split("\"")[0]
    episodes = []
    for e in episodeList[1:]:
        title = e.split("\"")[0]
        tmpEpisode = [title, []]
        playType = e.split("streamstarter_html5\" title=\"")
        if len(playType) > 1:
            for i in range(1, len(playType)):
                lang = "UT" if "Japanischen Stream mit Untertiteln starten" in playType[i] else "GER"
                streamLink = playType[i].split("data-playlist=\"")[1].split("\"")[0]
                tmpEpisode[1].append((lang, streamLink))
        if len(tmpEpisode[1]) > 0:
            episodes.append(tmpEpisode)
    vC.episodesToDownload = []
    while True:
        print("-1: Download starten")
        for e in range(len(episodes)):
            if isFilm:
                print(f"{e}: {episodes[e][1][0][0]}")
            else:
                print(f"{e}: {episodes[e][0]}")
        i = int(input(">"))
        if i == -1:
            break
        else:
            if isFilm:
                vC.episodesToDownload.append((f"{curr[0]} - {episodes[i][1][0][0]}", episodes[i][1][0][1]))
            else:
                for l in range(len(episodes[i][1])):
                	print(f"{l}: {episodes[i][1][l][0]}")
                i0 = int(input(">"))
                vC.episodesToDownload.append((f"{episodes[i][0]} - {episodes[i][1][i0][0]}", episodes[i][1][i0][1]))
    vC.outdir = input("Speicherort: ")
    for e in vC.episodesToDownload:
        print(f"=========={e[0]}==========")
        downloadEpisode(e[1], CSRF, f"https://www.anime-on-demand.de/anime/{curr[1]}", e[0])