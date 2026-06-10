const STORAGE_KEY = 'BEEST_saved_worksheets';

//Get all saved worksheets
export const getSavedWorksheets = () => {
    const data = localStorage.getItem(STORAGE_KEY);
    return data ? JSON.parse(data) : [];
};

//Save new worksheet
export const saveWorksheet = (title, language, criteria, words) => {
    const existingWorksheets = getSavedWorksheets();

    //Worksheet fields
    const newWorksheet = {
        id: crypto.randomUUID(),
        title: title,
        language: language,
        createdAt: new Date().toISOString(),
        creationCriteria: criteria,
        words: words
    };

    //Add into array of worksheets, push to user local storage
    existingWorksheets.push(newWorksheet);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(existingWorksheets));

    return newWorksheet;
};



