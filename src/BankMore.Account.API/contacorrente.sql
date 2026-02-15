CREATE TABLE contacorrente (
	idcontacorrente TEXT(37) PRIMARY KEY, -- id da conta corrente
	numero INTEGER(10) NOT NULL UNIQUE, -- numero da conta corrente
	nome TEXT(100) NOT NULL, -- nome do titular da conta corrente
	cpf TEXT(11) NOT NULL UNIQUE, -- cpf do titular
	ativo INTEGER(1) NOT NULL default 0, -- indicativo se a conta esta ativa. (0 = inativa, 1 = ativa).
	senha TEXT(100) NOT NULL,
	salt TEXT(100) NOT NULL,
	CHECK (ativo in (0,1))
);

CREATE TABLE movimento (
	idmovimento TEXT(37) PRIMARY KEY, -- id do movimento
	idcontacorrente TEXT(37) NOT NULL, -- id da conta corrente
	datamovimento TEXT(25) NOT NULL, -- data do movimento no formato DD/MM/YYYY HH:MM:SS
	tipomovimento TEXT(1) NOT NULL, -- C = Credito, D = Debito
	valor REAL NOT NULL, -- valor do movimento. Usar duas casas decimais.
	FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente),
	CHECK (tipomovimento in ('C','D'))
);

CREATE TABLE idempotencia (
	chave_idempotencia TEXT(37) PRIMARY KEY, -- identificacao chave de idempotencia
	requisicao TEXT(1000), -- dados de requisicao
	resultado TEXT(1000) -- dados de retorno
);
