using MediaNest.Shared.Entities;

namespace MediaNest.Shared.Services;

public class BulletinService(EntityRepository<BulletinItem> _items ) {
    public async Task<BulletinItem> GetBulletin(string id) {
        return await _items.GetById(id);
    }

    public async Task<List<BulletinItem>> GetBulletinsByPage(int page, int count) {
        return await _items.GetByPage(page, count);
    }

    public async Task<List<BulletinItem>> SearchBulletins(string term) {
        return await _items.Search(term);
    }

    public async Task<int> GetBulletinCount() {
        return await _items.GetCount();
    }

    public async Task<List<BulletinItem>> GetRandomBulletins(int count) {
        return await _items.GetRandom(count);
    }

    public async Task CreateBulletin(BulletinItem bulletin) {
        await _items.Create(bulletin);
    }

    public async Task UpdateBulletin(string id, BulletinItem updated) {
        await _items.Update(id, updated);
    }

    public async Task DeleteBulletin(string id) {
        await _items.Delete(id);
    }
}