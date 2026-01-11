// JavaScript source code
// 切換到 MediaNest DB
db = db.getSiblingDB('MediaNest');

if (!db.getCollectionNames().includes("Accounts")) {
    db.createCollection("Accounts");
    print("✅ Accounts collection created");
}

if (!db.getCollectionNames().includes("UserConfigs")) {
    db.createCollection("UserConfigs");
    print("✅ UserConfigs collection created");
}

if (!db.getCollectionNames().includes("Bulletins")) {
    db.createCollection("Bulletins");
    print("✅ Bulletins collection created");
}

if (!db.getCollectionNames().includes("Figures")) {
    db.createCollection("Figures");
    print("✅ Figures collection created");
}

if (!db.getCollectionNames().includes("Comics")) {
    db.createCollection("Comics");
    print("✅ Comics collection created");
}

if (!db.getCollectionNames().includes("ComicLists")) {
    db.createCollection("ComicLists");
    print("✅ ComicLists collection created");
}

if (!db.getCollectionNames().includes("Videos")) {
    db.createCollection("Videos");
    print("✅ Videos collection created");
}

if (!db.getCollectionNames().includes("VideoLists")) {
    db.createCollection("VideoLists");
    print("✅ VideoLists collection created");
}

if (!db.getCollectionNames().includes("Musics")) {
    db.createCollection("Musics");
    print("✅ Musics collection created");
}

if (!db.getCollectionNames().includes("MusicsList")) {
    db.createCollection("MusicsList");
    print("✅ MusicsList collection created");
}
