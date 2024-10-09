var candidateIndex = @Model.Candidates.Count; // Починаємо з кількості існуючих кандидатів

// Функція для видалення кандидата
function removeCandidate(index) {
    $('#candidate-' + index).remove();
}

// Функція для додавання нового кандидата
$('#add-candidate-btn').click(function () {
    var candidateForm = `
            <div class="candidate-form-group" id="candidate-${candidateIndex}">
                <h4>Кандидат ${candidateIndex + 1}</h4>
                <div class="form-group">
                    <label>Ім'я кандидата</label>
                    <input name="Candidates[${candidateIndex}].Name" class="form-control" />
                </div>

                <div class="form-group">
                    <label>Опис кандидата</label>
                    <input name="Candidates[${candidateIndex}].Description" class="form-control" />
                </div>

                <div class="form-group">
                    <label>Зображення кандидата</label>
                    <input type="file" name="Candidates[${candidateIndex}].ImageFile" class="form-control" />
                </div>

                <button type="button" class="btn btn-danger remove-candidate-btn" onclick="removeCandidate(${candidateIndex})">Видалити кандидата</button>
            </div>
        `;
    $('#candidate-form-container').append(candidateForm);
    candidateIndex++;
});