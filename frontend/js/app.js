const API_URL = window.API_URL || 'https://d37rkfntjs33w3.cloudfront.net/api/produtos';

async function carregarProdutos() {
    const content = document.getElementById('content');
    content.innerHTML = '<div class="loading">Carregando produtos...</div>';

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error('Erro ao carregar produtos');

        const produtos = await response.json();
        renderizarTabela(produtos);
    } catch (error) {
        content.innerHTML = `
            <div class="empty-state">
                <p>Erro ao conectar com a API</p>
                <p style="font-size: 0.85rem; color: #94a3b8;">${error.message}</p>
                <button class="btn btn-primary" onclick="carregarProdutos()">Tentar novamente</button>
            </div>`;
    }
}

function renderizarTabela(produtos) {
    const content = document.getElementById('content');

    if (produtos.length === 0) {
        content.innerHTML = `
            <div class="empty-state">
                <p>Nenhum produto cadastrado</p>
                <button class="btn btn-primary" onclick="abrirModalCriar()">Cadastrar primeiro produto</button>
            </div>`;
        return;
    }

    const rows = produtos.map(p => `
        <tr>
            <td>${escapeHtml(p.nome)}</td>
            <td>${escapeHtml(p.categoria)}</td>
            <td class="price">R$ ${Number(p.preco).toFixed(2)}</td>
            <td>${p.quantidadeEstoque}</td>
            <td><span class="badge ${p.ativo ? 'badge-active' : 'badge-inactive'}">${p.ativo ? 'Ativo' : 'Inativo'}</span></td>
            <td class="actions">
                <button class="btn btn-secondary btn-sm" onclick='abrirModalEditar(${JSON.stringify(p)})'>Editar</button>
                <button class="btn btn-danger btn-sm" onclick='confirmarExclusao("${p.id}", "${escapeHtml(p.nome)}")'>Excluir</button>
            </td>
        </tr>
    `).join('');

    content.innerHTML = `
        <div class="table-wrapper">
            <table>
                <thead>
                    <tr>
                        <th>Nome</th>
                        <th>Categoria</th>
                        <th>Preço</th>
                        <th>Estoque</th>
                        <th>Status</th>
                        <th>Ações</th>
                    </tr>
                </thead>
                <tbody>${rows}</tbody>
            </table>
        </div>`;
}

function abrirModalCriar() {
    document.getElementById('modal-title').textContent = 'Novo Produto';
    document.getElementById('produto-form').reset();
    document.getElementById('produto-id').value = '';
    document.getElementById('modal').classList.add('active');
}

function abrirModalEditar(produto) {
    document.getElementById('modal-title').textContent = 'Editar Produto';
    document.getElementById('produto-id').value = produto.id;
    document.getElementById('nome').value = produto.nome;
    document.getElementById('descricao').value = produto.descricao || '';
    document.getElementById('preco').value = produto.preco;
    document.getElementById('categoria').value = produto.categoria;
    document.getElementById('quantidade').value = produto.quantidadeEstoque;
    document.getElementById('imagemUrl').value = produto.imagemUrl || '';
    document.getElementById('modal').classList.add('active');
}

function fecharModal() {
    document.getElementById('modal').classList.remove('active');
}

async function salvarProduto(event) {
    event.preventDefault();

    const id = document.getElementById('produto-id').value;
    const dados = {
        nome: document.getElementById('nome').value,
        descricao: document.getElementById('descricao').value,
        preco: parseFloat(document.getElementById('preco').value),
        categoria: document.getElementById('categoria').value,
        quantidadeEstoque: parseInt(document.getElementById('quantidade').value),
        imagemUrl: document.getElementById('imagemUrl').value || null
    };

    if (id) {
        dados.ativo = true;
    }

    const btnSalvar = document.getElementById('btn-salvar');
    btnSalvar.disabled = true;
    btnSalvar.textContent = 'Salvando...';

    try {
        const url = id ? `${API_URL}/${id}` : API_URL;
        const method = id ? 'PUT' : 'POST';

        const response = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dados)
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.errors?.join(', ') || 'Erro ao salvar produto');
        }

        fecharModal();
        mostrarAlerta(id ? 'Produto atualizado com sucesso!' : 'Produto cadastrado com sucesso!', 'success');
        carregarProdutos();
    } catch (error) {
        mostrarAlerta(error.message, 'error');
    } finally {
        btnSalvar.disabled = false;
        btnSalvar.textContent = 'Salvar';
    }
}

function confirmarExclusao(id, nome) {
    document.getElementById('nome-excluir').textContent = nome;
    document.getElementById('modal-excluir').classList.add('active');

    const btnConfirmar = document.getElementById('btn-confirmar-excluir');
    const novoBotao = btnConfirmar.cloneNode(true);
    btnConfirmar.parentNode.replaceChild(novoBotao, btnConfirmar);

    novoBotao.addEventListener('click', () => excluirProduto(id));
}

function fecharModalExcluir() {
    document.getElementById('modal-excluir').classList.remove('active');
}

async function excluirProduto(id) {
    try {
        const response = await fetch(`${API_URL}/${id}`, { method: 'DELETE' });
        if (!response.ok) throw new Error('Erro ao excluir produto');

        fecharModalExcluir();
        mostrarAlerta('Produto excluído com sucesso!', 'success');
        carregarProdutos();
    } catch (error) {
        mostrarAlerta(error.message, 'error');
    }
}

function mostrarAlerta(mensagem, tipo) {
    const container = document.getElementById('alert-container');
    const alert = document.createElement('div');
    alert.className = `alert alert-${tipo}`;
    alert.textContent = mensagem;
    container.innerHTML = '';
    container.appendChild(alert);

    setTimeout(() => alert.remove(), 4000);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', carregarProdutos);
