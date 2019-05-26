import requests, json, os, urllib.request
from tkinter import *
from tkinter.filedialog import askdirectory

class varClass:
    def __init__(self):
        self.episodesToDownload = []
        self.username = ""
        self.password = ""
        self.outdir = ""
        self.selectedResolution = 0

vC = varClass()

class episodeSelect:
    def __init__(self, master, episodes):
        self.master = master
        self.episodes = episodes
        self.info = Label(master, text="Episoden auswählen:")
        self.info.grid(row=0, column=0, sticky=W)
        self.varlist = [[BooleanVar() for i0 in range(len(episodes[i][1]))] for i in range(len(episodes))]
        self.cblist = [[Checkbutton(master, text=episodes[i][1][i0][0], variable=self.varlist[i][i0]) for i0 in range(len(episodes[i][1]))] for i in range(len(episodes))]
        self.namelist = [Label(master, text=episodes[i][0]) for i in range(len(episodes))]
        for i in range(len(episodes)):
            self.namelist[i].grid(row=i, column=0, sticky=W)
            for i0 in range(len(self.cblist[i])):
                self.cblist[i][i0].grid(row=i, column=i0+1, sticky=W)
        self.downloadButton = Button(master, text="Weiter", command=self.returnEpisodes)
        self.downloadButton.grid(row=len(episodes)+2, column=0, sticky=W)

    def returnEpisodes(self):
        vC.episodesToDownload = []
        for vl in range(len(self.varlist)):
            for v in range(len(self.varlist[vl])):
                if self.varlist[vl][v].get():
                    vC.episodesToDownload.append((f"{self.episodes[vl][0]} - {self.episodes[vl][1][v][0]}", self.episodes[vl][1][v][1]))
                    print(f"{self.episodes[vl][0]} - {self.episodes[vl][1][v][0]}")
        self.master.destroy()

class resolutionSelect:
    def __init__(self, master, availResolutions):
        self.master = master
        self.availResolutions = [q[0] for q in availResolutions]
        self.info = Label(master, text="Gewünschte Auflösung wählen:")
        self.info.grid(row=0, column=0, sticky=W)
        self.resvar = StringVar(master)
        self.resvar.set(self.availResolutions[0])
        self.resmenu = OptionMenu(master, self.resvar, *self.availResolutions)
        self.resmenu.grid(row=1, column=0, sticky=W)
        self.cbutton = Button(master, text="Download starten", command=self.close)
        self.cbutton.grid(row=2, column=0, sticky=W)

    def close(self):
        vC.selectedResolution = self.availResolutions.index(self.resvar.get())
        self.master.destroy()

class loginDialog:
    def __init__(self, master):
        self.master = master
        self.uninfo = Label(master, text="Benutzer:")
        self.uninfo.grid(row=0, column=0, sticky=W)
        self.unentry = Entry(master)
        self.unentry.grid(row=0, column=1, sticky=W)
        self.uninfo = Label(master, text="Passwort:")
        self.uninfo.grid(row=1, column=0, sticky=W)
        self.pwentry = Entry(master)
        self.pwentry.grid(row=1, column=1, sticky=W)
        self.rmvar = BooleanVar()
        self.rmcb = Checkbutton(master, text="Anmeldedaten speichern", variable=self.rmvar)
        self.rmcb.grid(row=2, column=0, sticky=W)
        self.loginbutton = Button(master, text="Anmelden", command=self.login)
        self.loginbutton.grid(row=3, column=0, sticky=W)

    def login(self):
        vC.username = self.unentry.get()
        vC.password = self.pwentry.get()
        if self.rmvar.get():
            open("credentials.cfg", "w").write(f"{vC.username}\n{vC.password}")
        self.master.destroy()

session = requests.session()
session.get("https://anime-on-demand.de")
auth_token = str(session.get("https://www.anime-on-demand.de/users/sign_in").content).split("name=\"authenticity_token\" value=\"")[1].split("\"")[0]
print(f"Auth Token: {auth_token}")
if os.path.isfile("credentials.cfg"):
    creds = open("credentials.cfg", "r").readlines()
    vC.username = creds[0][:-1]
    vC.password = creds[1]
else:
    root = Tk()
    loginDialog(root)
    root.mainloop()
print(vC.username)
print(vC.password)
login = {"utf8": "✓",
         "authenticity_token": auth_token,
         "user[login]": vC.username,
         "user[password]": vC.password,
         "user[remember_me]": "1",
         "commit": "Einloggen"}
result = str(session.post("https://www.anime-on-demand.de/users/sign_in", login).content)
if vC.username in result:
    print("Eingeloggt!")
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
    root = Tk()
    resolutionSelect(root, availResolutions)
    root.mainloop()
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
    root = Tk()
    episodeSelect(root, episodes)
    root.mainloop()
    vC.outdir = askdirectory(title="Speicherort auswählen...")
    for e in vC.episodesToDownload:
        downloadEpisode(e[1], CSRF, f"https://www.anime-on-demand.de/anime/{curr[1]}", e[0])
