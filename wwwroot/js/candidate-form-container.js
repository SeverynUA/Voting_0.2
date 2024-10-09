window.onload = function () {
    let candidateCount = 1; // Лічильник для унікальних ID
    let totalCandidates = 1; // Загальна кількість кандидатів для нумерації на екрані

    // Додавання нового кандидата
    document.getElementById('add-candidate-btn').addEventListener('click', function () {
        totalCandidates++; // Кількість кандидатів на екрані
        candidateCount++; // Генеруємо новий унікальний ID для кожного кандидата

        const candidateFormGroup = `
        <div class="candidate-form-group" id="candidate-${candidateCount}">
            <h4>Кандидат ${totalCandidates}</h4>
            <div class="form-group">
                <label>Ім'я кандидата</label>
                <input name="Candidates[${candidateCount - 1}].Name" class="form-control" />
            </div>
            <div class="form-group">
                <label>Опис кандидата</label>
                <input name="Candidates[${candidateCount - 1}].Description" class="form-control" />
            </div>
            <div class="form-group">
                <label>Зображення кандидата</label>
                <input type="file" name="Candidates[${candidateCount - 1}].ImageFile" class="form-control" />
            </div>
            <button type="button" class="btn btn-danger remove-candidate-btn" data-id="${candidateCount}">Видалити кандидата</button>
        </div>
        `;
        document.getElementById('candidate-form-container').insertAdjacentHTML('beforeend', candidateFormGroup);
    });

    // Делегування подій: обробляємо події кліку на кнопках через батьківський контейнер
    document.getElementById('candidate-form-container').addEventListener('click', function (event) {
        if (event.target.classList.contains('remove-candidate-btn')) {
            const candidateId = event.target.getAttribute('data-id');
            const candidateElement = document.getElementById(`candidate-${candidateId}`);
            if (candidateElement) {
                candidateElement.remove(); // Видаляємо елемент кандидата
                totalCandidates--; // Зменшуємо кількість кандидатів на екрані
            }
        }
    });
};
