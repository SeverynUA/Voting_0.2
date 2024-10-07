<script>
    let candidateCount = 1; // Лічильник для додавання нових кандидатів

    document.getElementById('add-candidate-btn').addEventListener('click', function () {
        candidateCount++; // Збільшуємо лічильник

    // Створюємо нові поля для наступного кандидата
    const candidateFormGroup = `
    <div class="candidate-form-group">
        <h4>Кандидат ${candidateCount}</h4>
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
    </div>
    `;

    // Додаємо нові поля в контейнер
    document.getElementById('candidate-form-container').insertAdjacentHTML('beforeend', candidateFormGroup);
    });
</script>
