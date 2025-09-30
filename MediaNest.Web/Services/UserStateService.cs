namespace MediaNest.Web.Services; 
public class UserStateService(ApiClient client) {
    private List<string> selectedComic = [];
    public void AddComic(string id) {
        if (!selectedComic.Contains(id)) {
            selectedComic.Add(id);
        }
    }
    public void RemoveComic(string id) {
        if (selectedComic.Contains(id)) {
            selectedComic.Remove(id);
        }
    }
    
}
