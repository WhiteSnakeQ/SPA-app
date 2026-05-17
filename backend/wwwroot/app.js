document.getElementById("commentForm").addEventListener("submit", async (e) => {

    e.preventDefault();

    const formData = new FormData();

    formData.append(
        "UserName",
        document.querySelector("[name=username]").value
    );

    formData.append(
        "Email",
        document.querySelector("[name=email]").value
    );

    formData.append(
        "Text",
        document.querySelector("[name=text]").value
    );

    formData.append(
        "Homepage",
        document.querySelector("[name=homepage]").value
    );

    for (const file of fileInput.files) {
        formData.append("Files", file);
    }

    const response = await fetch("/api/comments", {
        method: "POST",
        body: formData
    });

    console.log(await response.text());
});