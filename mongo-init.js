// JavaScript source code
// 切換到 MediaNest DB
db = db.getSiblingDB('MediaNest');

// 建立 Accounts collection
if (!db.getCollectionNames().includes("Accounts")) {
    db.createCollection("Accounts");
    print("✅ Accounts collection created");
}

// 建立 Comics collection
if (!db.getCollectionNames().includes("Comics")) {
    db.createCollection("Comics");
    print("✅ Comics collection created");
}